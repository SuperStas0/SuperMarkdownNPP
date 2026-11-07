using System;
using System.Windows.Forms;

namespace SuperMarkdown
{
    internal sealed class Overlay
    {
        readonly SuperMarkdownPanel panel;
        readonly SciHook hook = new SciHook();
        IntPtr host;
        bool shown, stripped, vBar, hBar;

        public Overlay(SuperMarkdownPanel panel)
        {
            this.panel = panel;
            hook.Resized = Fit;
            hook.Focused = Redirect;
        }

        public SuperMarkdownPanel Panel
        {
            get { return panel; }
        }

        public bool Shown
        {
            get { return shown; }
        }

        public void Show(IntPtr sci, bool focus)
        {
            if (!shown || host != sci)
            {
                IntPtr h = panel.Handle;
                if (host != sci)
                {
                    Restore();
                    Native.ShowWindow(h, Native.SW_HIDE); // no flash at the default size while re-parenting
                    hook.ReleaseHandle();

                    int style = Native.GetWindowLong(sci, Native.GWL_STYLE);
                    if ((style & Native.WS_CLIPCHILDREN) == 0)
                        Native.SetWindowLong(sci, Native.GWL_STYLE, style | Native.WS_CLIPCHILDREN);

                    Native.SetParent(h, sci);
                    hook.AssignHandle(sci);
                    host = sci;
                }
                Strip();
                shown = true;
                Fit();
            }
            if (focus || Native.GetFocus() == sci) panel.FocusWeb();
        }

        public void Hide()
        {
            if (!shown) return;
            shown = false;
            Restore();
            Native.ShowWindow(panel.Handle, Native.SW_HIDE);
        }

        // The editor's own scrollbars sit outside its client area, so the overlay cannot cover them.
        void Strip()
        {
            if (stripped) return;
            vBar = Native.SendMessage(host, Native.SCI_GETVSCROLLBAR, IntPtr.Zero, IntPtr.Zero) != IntPtr.Zero;
            hBar = Native.SendMessage(host, Native.SCI_GETHSCROLLBAR, IntPtr.Zero, IntPtr.Zero) != IntPtr.Zero;
            Native.SendMessage(host, Native.SCI_SETVSCROLLBAR, IntPtr.Zero, IntPtr.Zero);
            Native.SendMessage(host, Native.SCI_SETHSCROLLBAR, IntPtr.Zero, IntPtr.Zero);
            stripped = true;
        }

        void Restore()
        {
            if (!stripped) return;
            stripped = false;
            Native.SendMessage(host, Native.SCI_SETVSCROLLBAR, (IntPtr)(vBar ? 1 : 0), IntPtr.Zero);
            Native.SendMessage(host, Native.SCI_SETHSCROLLBAR, (IntPtr)(hBar ? 1 : 0), IntPtr.Zero);
        }

        void Fit()
        {
            Rect r;
            if (!shown || !Native.GetClientRect(host, out r)) return;
            Native.SetWindowPos(panel.Handle, Native.HWND_TOP, 0, 0, r.Right - r.Left, r.Bottom - r.Top, Native.SWP_NOACTIVATE | Native.SWP_SHOWWINDOW);
        }

        // Keystrokes must not land in the editor hidden underneath.
        void Redirect()
        {
            if (shown) panel.BeginInvoke(new Action(panel.FocusWeb));
        }
    }

    internal sealed class SciHook : NativeWindow
    {
        const int WM_SIZE = 0x0005;
        const int WM_SETFOCUS = 0x0007;

        public Action Resized, Focused;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            Action h = m.Msg == WM_SIZE ? Resized : m.Msg == WM_SETFOCUS ? Focused : null;
            if (h == null) return;
            try { h(); } catch { }
        }
    }
}
