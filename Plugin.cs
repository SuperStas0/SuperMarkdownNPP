using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SuperMarkdown
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void PluginFunc();

    internal static class Plugin
    {
        internal const string PluginName = "SuperMarkdown++";

        static IntPtr npp, sci0, sci1;
        static IntPtr funcs, nameBuf;
        static SuperMarkdownPanel panel;
        static System.Windows.Forms.Timer timer;
        static string theme = "dark";
        static string style = Styles.Default;
        static int funcSize, cmdOffset;
        static string lastText, lastPath;
        static bool wanted, internalChange;

        // Kept in static fields so the GC never collects the native thunks.
        // A null callback makes Notepad++ draw a separator. The style items start at StyleFirst, in Styles.Names order.
        const int StyleFirst = 3;
        static readonly PluginFunc[] callbacks = { ToggleCommand, ThemeCommand, null, StyleGitHubCommand, StyleGitLabCommand, StyleForgejoCommand };
        static readonly string[] menuNames = { "Toggle SuperMarkdown++", "Toggle Light/Dark Theme", "-", "GitHub Style", "GitLab Style", "Forgejo Style" };

        // ShortcutKey: bool _isCtrl, _isAlt, _isShift; UCHAR _key. Ctrl+Shift+M for the toggle.
        static readonly byte[][] shortcuts = { new byte[] { 1, 0, 1, 0x4D }, null, null, null, null, null };

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
            style = Settings.LoadStyle();
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
            funcSize = size;
            cmdOffset = cmdOff;

            int total = size * menuNames.Length;
            funcs = Marshal.AllocHGlobal(total);
            Marshal.Copy(new byte[total], 0, funcs, total);

            for (int i = 0; i < menuNames.Length; i++)
            {
                IntPtr item = IntPtr.Add(funcs, i * size);
                char[] chars = menuNames[i].ToCharArray();
                Marshal.Copy(chars, 0, item, Math.Min(chars.Length, 63));
                if (callbacks[i] != null)
                    Marshal.WriteIntPtr(item, 128, Marshal.GetFunctionPointerForDelegate(callbacks[i]));

                if (i >= StyleFirst && i - StyleFirst == Styles.IndexOf(style))
                    Marshal.WriteByte(item, chkOff, 1);

                if (shortcuts[i] != null)
                {
                    IntPtr key = Marshal.AllocHGlobal(shortcuts[i].Length);
                    Marshal.Copy(shortcuts[i], 0, key, shortcuts[i].Length);
                    Marshal.WriteIntPtr(item, keyOff, key);
                }
            }
        }

        static void ToggleCommand()
        {
            try
            {
                if (panel == null) return;
                wanted = !wanted;
                SetDock(wanted);
                if (wanted) Schedule(1);
            }
            catch { }
        }

        static void ThemeCommand()
        {
            try
            {
                theme = theme == "dark" ? "light" : "dark";
                Settings.SaveTheme(theme);
                if (panel != null) panel.SetLook(style, theme);
            }
            catch { }
        }

        static void StyleGitHubCommand() { SetStyle("github"); }

        static void StyleGitLabCommand() { SetStyle("gitlab"); }

        static void StyleForgejoCommand() { SetStyle("forgejo"); }

        static void SetStyle(string s)
        {
            try
            {
                style = Styles.Normalize(s);
                Settings.SaveStyle(style);
                if (panel != null) panel.SetLook(style, theme);

                // Notepad++ assigns each item's command ID after getFuncsArray.
                for (int k = 0; k < Styles.Names.Length; k++)
                {
                    int id = Marshal.ReadInt32(funcs, (StyleFirst + k) * funcSize + cmdOffset);
                    Native.SendMessage(npp, Native.NPPM_SETMENUITEMCHECK, (IntPtr)id, (IntPtr)(Styles.Names[k] == style ? 1 : 0));
                }
            }
            catch { }
        }

        // ---- panel ---------------------------------------------------------

        static void RegisterPanel()
        {
            if (panel != null) return;

            panel = new SuperMarkdownPanel(style, theme);
            panel.BecameVisible += delegate { wanted = true; Schedule(30); };
            panel.BecameHidden += delegate { if (!internalChange) wanted = false; };

            timer = new System.Windows.Forms.Timer();
            timer.Tick += delegate { Refresh(); };

            var tb = new TbData();
            tb.hClient = panel.Handle; // forces handle creation
            tb.pszName = Marshal.StringToHGlobalUni(PluginName);
            tb.dlgID = 0;
            tb.uMask = Native.DWS_DF_CONT_RIGHT;
            tb.iPrevCont = -1;
            tb.pszModuleName = Marshal.StringToHGlobalUni("SuperMarkdown++.dll");

            IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf<TbData>());
            Marshal.StructureToPtr(tb, p, false);
            Native.SendMessage(npp, Native.NPPM_DMMREGASDCKDLG, IntPtr.Zero, p);

            panel.StartWeb();
        }

        static void SetDock(bool show)
        {
            internalChange = true;
            try
            {
                Native.SendMessage(npp, show ? Native.NPPM_DMMSHOW : Native.NPPM_DMMHIDE, IntPtr.Zero, panel.Handle);
            }
            finally
            {
                internalChange = false;
            }
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
            if (panel == null) return;

            bool visible = Native.IsWindowVisible(panel.Handle);
            if (visible) wanted = true;
            if (!visible && !wanted) return;

            try
            {
                string path = GetCurrentPath();
                if (!IsMarkdown(path))
                {
                    if (visible)
                    {
                        lastText = null;
                        SetDock(false);
                    }
                    return;
                }

                if (!visible)
                {
                    SetDock(true);
                    return;
                }

                string text = GetText();
                if (text == lastText && path == lastPath) return;
                lastText = text;
                lastPath = path;

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
                panel.ShowMessage("SuperMarkdown++ error: " + ex.Message);
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
        static string FilePathFor(string name)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SuperMarkdown++", name);
        }

        static string Load(string name)
        {
            try
            {
                string path = FilePathFor(name);
                if (File.Exists(path)) return File.ReadAllText(path).Trim();
            }
            catch { }
            return "";
        }

        static void Save(string name, string value)
        {
            try
            {
                string path = FilePathFor(name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, value);
            }
            catch { }
        }

        public static string LoadTheme()
        {
            return Load("theme.txt") == "light" ? "light" : "dark";
        }

        public static void SaveTheme(string theme)
        {
            Save("theme.txt", theme);
        }

        public static string LoadStyle()
        {
            return Styles.Normalize(Load("style.txt"));
        }

        public static void SaveStyle(string style)
        {
            Save("style.txt", style);
        }
    }
}
