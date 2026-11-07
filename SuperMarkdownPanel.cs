using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace SuperMarkdown
{
    internal sealed class SuperMarkdownPanel : UserControl
    {
        const string Host = "md.local";
        const int WM_SHOWWINDOW = 0x0018;

        readonly WebView2 web = new WebView2();
        readonly Label error = new Label();
        bool started, ready;
        string style, theme;
        string html = "";
        string name, keys = "[]";
        string dir, mappedDir;

        public event Action BecameVisible;
        public event Action BecameHidden;
        public event Action<int> Command;

        public SuperMarkdownPanel(string style, string theme)
        {
            this.style = style;
            this.theme = theme;
            BackColor = Styles.Background(style, theme);

            web.Dock = DockStyle.Fill;
            web.DefaultBackgroundColor = BackColor;
            Controls.Add(web);

            error.Dock = DockStyle.Fill;
            error.Visible = false;
            error.Padding = new Padding(12);
            error.ForeColor = Color.Gray;
            Controls.Add(error);
        }

        public async void StartWeb()
        {
            if (started) return;
            started = true;
            try
            {
                string userData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SuperMarkdown++", "WebView2");
                CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(null, userData);
                await web.EnsureCoreWebView2Async(env);

                CoreWebView2 cw = web.CoreWebView2;
                cw.Settings.AreDevToolsEnabled = false;
                cw.NavigationStarting += OnNavigationStarting;
                cw.NewWindowRequested += OnNewWindowRequested;
                cw.NavigationCompleted += OnNavigationCompleted;
                cw.WebMessageReceived += OnWebMessage;
                cw.NavigateToString(Renderer.Shell(style, theme));
            }
            catch (Exception ex)
            {
                error.Text = "SuperMarkdown++ could not start WebView2.\r\n\r\n" + ex.Message +
                    "\r\n\r\nInstall the Evergreen WebView2 Runtime: https://developer.microsoft.com/microsoft-edge/webview2/";
                error.Visible = true;
                web.Visible = false;
            }
        }

        public void Render(string bodyHtml, string baseDir, string fileName)
        {
            html = bodyHtml;
            dir = baseDir;
            name = fileName;
            Push();
        }

        public void ShowMessage(string message)
        {
            Render("<p class=\"msg\">" + WebUtility.HtmlEncode(message) + "</p>", null, null);
        }

        // JSON array of [ctrl, alt, shift, virtual key, menu index]; lets the page run the same shortcuts while it has focus.
        public void SetKeys(string json)
        {
            keys = json;
            if (!ready) return;
            try { web.CoreWebView2.ExecuteScriptAsync("setKeys(" + keys + ")"); } catch { }
        }

        public void FocusWeb()
        {
            web.Focus();
        }

        public void SetLook(string s, string t)
        {
            style = s;
            theme = t;
            BackColor = Styles.Background(s, t);
            try
            {
                web.DefaultBackgroundColor = BackColor;
                if (ready) web.CoreWebView2.ExecuteScriptAsync("setLook('" + s + "','" + t + "')");
            }
            catch { }
        }

        void Push()
        {
            if (!ready) return;
            try
            {
                CoreWebView2 cw = web.CoreWebView2;
                if (dir != mappedDir)
                {
                    if (mappedDir != null) cw.ClearVirtualHostNameToFolderMapping(Host);
                    if (dir != null)
                        cw.SetVirtualHostNameToFolderMapping(Host, dir, CoreWebView2HostResourceAccessKind.Allow);
                    mappedDir = dir;
                }
                cw.ExecuteScriptAsync("setContent(" + JsonSerializer.Serialize(html) + "," + JsonSerializer.Serialize(name) + ")");
            }
            catch { }
        }

        void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // Also runs again if the user reloads the page from the context menu.
            ready = true;
            try { web.CoreWebView2.ExecuteScriptAsync("setLook('" + style + "','" + theme + "')"); } catch { }
            try { web.CoreWebView2.ExecuteScriptAsync("setKeys(" + keys + ")"); } catch { }
            Push();
        }

        void OnWebMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            int index;
            try
            {
                if (!int.TryParse(e.TryGetWebMessageAsString(), out index)) return;
            }
            catch
            {
                return;
            }

            Action<int> h = Command;
            if (h != null) h(index);
        }

        void OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            string uri = e.Uri ?? "";
            if (uri.StartsWith("data:", StringComparison.OrdinalIgnoreCase) || uri == "about:blank") return;

            e.Cancel = true;
            OpenExternal(uri);
        }

        void OnNewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            OpenExternal(e.Uri);
        }

        static void OpenExternal(string uri)
        {
            Uri u;
            if (!Uri.TryCreate(uri, UriKind.Absolute, out u)) return;
            if (u.Host.Equals(Host, StringComparison.OrdinalIgnoreCase)) return; // local relative links: ignored

            if (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps || u.Scheme == Uri.UriSchemeMailto)
            {
                try { Process.Start(new ProcessStartInfo(u.AbsoluteUri) { UseShellExecute = true }); }
                catch { }
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_SHOWWINDOW)
            {
                Action h = m.WParam != IntPtr.Zero ? BecameVisible : BecameHidden;
                if (h != null) h();
            }
        }
    }
}
