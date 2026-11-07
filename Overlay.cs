using System;
using System.Windows.Forms;

namespace SuperMarkdown
{
    internal sealed class Overlay
    {
        readonly SuperMarkdownPanel panel;
        readonly SciHook hook = new SciHook();
        IntPtr host;
        bool shown;

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
                    Native.ShowWindow(h, Native.SW_HIDE); // no flash at the default size while re-parenting
                    hook.ReleaseHandle();

                    int style = Native.GetWindowLong(sci, Native.GWL_STYLE);
                    if ((style & Native.WS_CLIPCHILDREN) == 0)
                        Native.SetWindowLong(sci, Native.GWL_STYLE, style | Native.WS_CLIPCHILDREN);

                    Native.SetParent(h, sci);
                    hook.AssignHandle(sci);
                    host = sci;
                }
                shown = true;
                Fit();
            }
            if (focus || Native.GetFocus() == sci) panel.FocusWeb();
        }

        public void Hide()
        {
            if (!shown) return;
            shown = false;
            Native.ShowWindow(panel.Handle, Native.SW_HIDE);
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
