using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MarkdownPreviewPlugin
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void PluginFunc();

    internal static class Plugin
    {
        internal const string PluginName = "Markdown Preview";

        static IntPtr npp, sci0, sci1;
        static IntPtr funcs, nameBuf;
        static PreviewPanel panel;
        static System.Windows.Forms.Timer timer;
        static string theme = "dark";
        static string lastText, lastPath;

        // Kept in static fields so the GC never collects the native thunks.
        static readonly PluginFunc[] callbacks = { ToggleCommand, ThemeCommand };
        static readonly string[] menuNames = { "Toggle Markdown Preview", "Toggle Light/Dark Theme" };

        // ---- exports -------------------------------------------------------

        internal static void SetInfo(NppData data)
        {
            npp = data.NppHandle;
            sci0 = data.ScintillaMainHandle;
            sci1 = data.ScintillaSecondHandle;

            // Notepad++ has no WinForms sync context; async/await in the panel needs one.
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            theme = Settings.LoadTheme();
            BuildMenu();
        }

        internal static IntPtr GetName()
        {
            if (nameBuf == IntPtr.Zero)
                nameBuf = Marshal.StringToHGlobalUni(PluginName);
            return nameBuf;
        }

        internal static IntPtr GetFuncsArray(IntPtr nbF)
        {
            Marshal.WriteInt32(nbF, menuNames.Length);
            return funcs;
        }

        internal static void BeNotified(IntPtr n)
        {
            IntPtr from = Marshal.ReadIntPtr(n, 0);
            uint code = (uint)Marshal.ReadInt32(n, IntPtr.Size * 2);

            switch (code)
            {
                case Native.NPPN_READY:
                    RegisterPanel();
                    break;
                case Native.NPPN_BUFFERACTIVATED:
                case Native.NPPN_LANGCHANGED:
                    Schedule(30);
                    break;
                case Native.NPPN_FILESAVED:
                    Schedule(100);
                    break;
                case Native.NPPN_SHUTDOWN:
                    if (timer != null) timer.Stop();
                    break;
                case Native.SCN_MODIFIED:
                    if (from == sci0 || from == sci1) Schedule(250);
                    break;
            }
        }

        // ---- menu ----------------------------------------------------------

        static int Align(int v, int a)
        {
            return (v + a - 1) / a * a;
        }

        static void BuildMenu()
        {
            // FuncItem: wchar_t _itemName[64]; void(*_pFunc)(); int _cmdID; bool _init2Check; ShortcutKey* _pShKey;
            int ptr = IntPtr.Size;
            int cmdOff = 128 + ptr;
            int chkOff = cmdOff + 4;
            int keyOff = Align(chkOff + 1, ptr);
            int size = Align(keyOff + ptr, ptr);

            int total = size * menuNames.Length;
            funcs = Marshal.AllocHGlobal(total);
            Marshal.Copy(new byte[total], 0, funcs, total);

            for (int i = 0; i < menuNames.Length; i++)
            {
                IntPtr item = IntPtr.Add(funcs, i * size);
                char[] chars = menuNames[i].ToCharArray();
                Marshal.Copy(chars, 0, item, Math.Min(chars.Length, 63));
                Marshal.WriteIntPtr(item, 128, Marshal.GetFunctionPointerForDelegate(callbacks[i]));
            }
        }

        static void ToggleCommand()
        {
            try
            {
                if (panel == null) return;
                bool visible = Native.IsWindowVisible(panel.Handle);
                Native.SendMessage(npp, visible ? Native.NPPM_DMMHIDE : Native.NPPM_DMMSHOW, IntPtr.Zero, panel.Handle);
                if (!visible) Schedule(1);
            }
            catch { }
        }

        static void ThemeCommand()
        {
            try
            {
                theme = theme == "dark" ? "light" : "dark";
                Settings.SaveTheme(theme);
                if (panel != null) panel.SetTheme(theme);
            }
            catch { }
        }

        // ---- panel ---------------------------------------------------------

        static void RegisterPanel()
        {
            if (panel != null) return;

            panel = new PreviewPanel(theme);
            panel.BecameVisible += delegate { Schedule(30); };

            timer = new System.Windows.Forms.Timer();
            timer.Tick += delegate { Refresh(); };

            var tb = new TbData();
            tb.hClient = panel.Handle; // forces handle creation
            tb.pszName = Marshal.StringToHGlobalUni(PluginName);
            tb.dlgID = 0;
            tb.uMask = Native.DWS_DF_CONT_RIGHT;
            tb.iPrevCont = -1;
            tb.pszModuleName = Marshal.StringToHGlobalUni("MarkdownPreview.dll");

            IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf<TbData>());
            Marshal.StructureToPtr(tb, p, false);
            Native.SendMessage(npp, Native.NPPM_DMMREGASDCKDLG, IntPtr.Zero, p);

            panel.StartWeb();
        }

        static void Schedule(int ms)
        {
            if (timer == null) return;
            timer.Stop();
            timer.Interval = ms;
            timer.Start();
        }

        static void Refresh()
        {
            if (timer != null) timer.Stop();
            if (panel == null || !Native.IsWindowVisible(panel.Handle)) return;

            try
            {
                string path = GetCurrentPath();
                string text = GetText();
                if (text == lastText && path == lastPath) return;
                lastText = text;
                lastPath = path;

                if (!IsMarkdown(path))
                {
                    panel.ShowMessage("Not a Markdown file.");
                    return;
                }

                string dir = null;
                if (Path.IsPathRooted(path))
                {
                    string d = Path.GetDirectoryName(path);
                    if (d != null && Directory.Exists(d)) dir = d;
                }
                panel.Render(Renderer.ToHtml(text), dir);
            }
            catch (Exception ex)
            {
                lastText = null;
                panel.ShowMessage("Preview error: " + ex.Message);
            }
        }

        // ---- Notepad++ / Scintilla access ----------------------------------

        static IntPtr CurrentScintilla()
        {
            int which = 0;
            Native.SendMessage(npp, Native.NPPM_GETCURRENTSCINTILLA, IntPtr.Zero, ref which);
            return which == 1 ? sci1 : sci0;
        }

        static string GetCurrentPath()
        {
            var sb = new StringBuilder(4096);
            Native.SendMessage(npp, Native.NPPM_GETFULLCURRENTPATH, (IntPtr)4096, sb);
            return sb.ToString();
        }

        static string GetText()
        {
            IntPtr s = CurrentScintilla();
            int len = (int)Native.SendMessage(s, Native.SCI_GETLENGTH, IntPtr.Zero, IntPtr.Zero);
            if (len <= 0) return "";

            IntPtr buf = Marshal.AllocHGlobal(len + 1);
            try
            {
                Native.SendMessage(s, Native.SCI_GETTEXT, (IntPtr)(len + 1), buf);
                byte[] bytes = new byte[len];
                Marshal.Copy(buf, bytes, 0, len);

                int cp = (int)Native.SendMessage(s, Native.SCI_GETCODEPAGE, IntPtr.Zero, IntPtr.Zero);
                Encoding enc = Encoding.UTF8;
                if (cp != 0 && cp != 65001)
                {
                    try { enc = Encoding.GetEncoding(cp); } catch { enc = Encoding.UTF8; }
                }
                return enc.GetString(bytes);
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }
        }

        static bool IsMarkdown(string path)
        {
            string ext = Path.GetExtension(path ?? "").ToLowerInvariant();
            if (ext.Length == 0) return true; // unsaved "new 1" tabs
            switch (ext)
            {
                case ".md":
                case ".markdown":
                case ".mdown":
                case ".mkd":
                case ".mkdn":
                case ".mdx":
                case ".rmd":
                    return true;
            }
            return false;
        }
    }

    internal static class Settings
    {
        static string FilePath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MarkdownPreview", "theme.txt");
            }
        }

        public static string LoadTheme()
        {
            try
            {
                if (File.Exists(FilePath) && File.ReadAllText(FilePath).Trim() == "light") return "light";
            }
            catch { }
            return "dark";
        }

        public static void SaveTheme(string theme)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, theme);
            }
            catch { }
        }
    }
}
