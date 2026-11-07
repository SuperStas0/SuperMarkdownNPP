using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
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
        static Overlay overlay;
        static System.Windows.Forms.Timer timer;
        static string theme = "dark";
        static string style = Styles.Default;
        static string mode = Modes.Hidden;
        static string shown = Modes.Sidebar;
        static int funcSize, cmdOffset;
        static string lastText, lastPath;
        static bool internalChange, closing;
        static string lastCmd;
        static int lastCmdTick;

        // Kept in static fields so the GC never collects the native thunks.
        // A null callback makes Notepad++ draw a separator. The mode items start at ModeFirst (Modes.Names order), the style items at StyleFirst (Styles.Names order).
        const int ModeFirst = 2;
        const int StyleFirst = 9;
        static readonly PluginFunc[] callbacks = { ToggleCommand, null, HiddenCommand, SidebarCommand, ReplaceCommand, null, ThemeCommand, NextStyleCommand, null, StyleGitHubCommand, StyleGitLabCommand, StyleForgejoCommand };
        static readonly string[] menuNames = { "Toggle SuperMarkdown++", "-", "Hidden", "Sidebar", "Replace Editor", "-", "Toggle Light/Dark Theme", "Next Style", "-", "GitHub Style", "GitLab Style", "Forgejo Style" };

        // ShortcutKey: bool _isCtrl, _isAlt, _isShift; UCHAR _key. Ctrl+Shift+M for the toggle.
        // Ctrl+Alt+Shift + Left/Right/Up/Down/PageDown for the rest: Ctrl+Shift+arrows are Notepad++'s own move-line and select-word keys.
        static readonly byte[][] shortcuts = { new byte[] { 1, 0, 1, 0x4D }, null, new byte[] { 1, 1, 1, 0x25 }, new byte[] { 1, 1, 1, 0x27 }, new byte[] { 1, 1, 1, 0x26 }, null, new byte[] { 1, 1, 1, 0x28 }, new byte[] { 1, 1, 1, 0x22 }, null, null, null, null };

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
            shown = Settings.LoadShown();
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
                    closing = true;
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

                if (i >= ModeFirst && i - ModeFirst == Array.IndexOf(Modes.Names, mode))
                    Marshal.WriteByte(item, chkOff, 1);

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

        // The panel runs the same shortcuts through the page, so one key press can arrive twice.
        static bool Repeat(string name)
        {
            int now = Environment.TickCount;
            bool again = name == lastCmd && unchecked(now - lastCmdTick) < 200;
            lastCmd = name;
            lastCmdTick = now;
            return again;
        }

        static string KeysJson()
        {
            List<int[]> keys = new List<int[]>();
            for (int i = 0; i < shortcuts.Length; i++)
            {
                byte[] k = shortcuts[i];
                if (k != null) keys.Add(new int[] { k[0], k[1], k[2], k[3], i });
            }
            return JsonSerializer.Serialize(keys);
        }

        static void RunCommand(int index)
        {
            if (index < 0 || index >= callbacks.Length || callbacks[index] == null) return;
            callbacks[index]();
        }

        static void ToggleCommand()
        {
            if (Repeat("toggle")) return;
            SetMode(mode == Modes.Hidden ? shown : Modes.Hidden);
        }

        static void HiddenCommand() { SetMode(Modes.Hidden); }

        static void SidebarCommand() { SetMode(Modes.Sidebar); }

        static void ReplaceCommand() { SetMode(Modes.Replace); }

        static void SetMode(string m)
        {
            try
            {
                if (panel == null) return;
                mode = Modes.Normalize(m);
                if (mode != Modes.Hidden) shown = mode;
                Settings.SaveMode(mode);
                Settings.SaveShown(shown);
                CheckModes();
                lastText = null;
                Refresh(true);
            }
            catch { }
        }

        static void CheckModes()
        {
            for (int k = 0; k < Modes.Names.Length; k++)
            {
                int id = Marshal.ReadInt32(funcs, (ModeFirst + k) * funcSize + cmdOffset);
                Native.SendMessage(npp, Native.NPPM_SETMENUITEMCHECK, (IntPtr)id, (IntPtr)(Modes.Names[k] == mode ? 1 : 0));
            }
        }

        static void ThemeCommand()
        {
            try
            {
                if (Repeat("theme")) return;
                theme = theme == "dark" ? "light" : "dark";
                Settings.SaveTheme(theme);
                if (panel != null) panel.SetLook(style, theme);
                if (overlay != null) overlay.Panel.SetLook(style, theme);
            }
            catch { }
        }

        static void NextStyleCommand()
        {
            if (Repeat("style")) return;
            SetStyle(Styles.Names[(Styles.IndexOf(style) + 1) % Styles.Names.Length]);
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
                if (overlay != null) overlay.Panel.SetLook(style, theme);

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
            panel.SetKeys(KeysJson());
            panel.Command += RunCommand;
            panel.BecameVisible += SidebarShown;
            panel.BecameHidden += SidebarHidden;

            timer = new System.Windows.Forms.Timer();
            timer.Tick += delegate { Refresh(false); };

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

            // The sidebar restores itself; Replace Editor is ours to restore.
            if (Settings.LoadMode() == Modes.Replace) SetMode(Modes.Replace);
        }

        // Notepad++ shows or hides the sidebar on its own (menu, close button, restored layout).
        static void SidebarShown()
        {
            if (!internalChange && !closing && mode != Modes.Sidebar)
            {
                mode = shown = Modes.Sidebar;
                Settings.SaveMode(mode);
                Settings.SaveShown(shown);
                CheckModes();
                if (overlay != null) overlay.Hide();
            }
            Schedule(30);
        }

        static void SidebarHidden()
        {
            if (internalChange || closing || mode != Modes.Sidebar) return;
            mode = Modes.Hidden;
            Settings.SaveMode(mode);
            CheckModes();
        }

        static void ShowOverlay(bool focus)
        {
            if (overlay == null)
            {
                SuperMarkdownPanel p = new SuperMarkdownPanel(style, theme);
                p.SetKeys(KeysJson());
                p.Command += RunCommand;
                overlay = new Overlay(p);
            }
            overlay.Show(CurrentScintilla(), focus);
            overlay.Panel.StartWeb();
        }

        static SuperMarkdownPanel ActivePanel()
        {
            return mode == Modes.Replace && overlay != null ? overlay.Panel : panel;
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

        static void Refresh(bool focus)
        {
            if (timer != null) timer.Stop();
            if (panel == null) return;

            bool sideNow = Native.IsWindowVisible(panel.Handle);
            bool overNow = overlay != null && overlay.Shown;
            if (mode == Modes.Hidden && !sideNow && !overNow) return;

            try
            {
                string path = GetCurrentPath();
                bool md = mode != Modes.Hidden && IsMarkdown(path);
                bool side = md && mode == Modes.Sidebar;
                bool over = md && mode == Modes.Replace;

                if (sideNow && !side) SetDock(false);
                if (overNow && !over) overlay.Hide();

                if (!md)
                {
                    lastText = null;
                    if (focus) Native.SetFocus(CurrentScintilla());
                    return;
                }

                if (side && !sideNow)
                {
                    SetDock(true);
                    return;
                }

                if (over) ShowOverlay(focus);

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
                ActivePanel().Render(Renderer.ToHtml(text), dir, Path.GetFileName(path));
            }
            catch (Exception ex)
            {
                lastText = null;
                ActivePanel().ShowMessage("SuperMarkdown++ error: " + ex.Message);
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

        public static string LoadMode()
        {
            return Modes.Normalize(Load("mode.txt"));
        }

        public static void SaveMode(string mode)
        {
            Save("mode.txt", mode);
        }

        public static string LoadShown()
        {
            string s = Modes.Normalize(Load("shown.txt"));
            return s == Modes.Hidden ? Modes.Sidebar : s;
        }

        public static void SaveShown(string mode)
        {
            Save("shown.txt", mode);
        }
    }
}
