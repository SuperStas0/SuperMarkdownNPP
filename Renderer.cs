using Markdig;
using Markdig.Extensions.AutoIdentifiers;

namespace SuperMarkdown
{
    internal static class Renderer
    {
        static readonly MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseTaskLists()
            .UseAutoLinks()
            .UseFootnotes()
            .UseEmphasisExtras()
            .UseEmojiAndSmiley()
            .UseAlertBlocks()
            .UseYamlFrontMatter()
            .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
            .Build();

        public static string ToHtml(string markdown)
        {
            return Markdown.ToHtml(markdown ?? "", pipeline);
        }

        public static string Shell(string style, string theme)
        {
            return Template.Replace("__CSS__", Css)
                .Replace("__STYLE__", Styles.Normalize(style))
                .Replace("__THEME__", theme == "light" ? "light" : "dark");
        }

        const string Template = """
<!DOCTYPE html>
<html data-style="__STYLE__" data-theme="__THEME__">
<head>
<meta charset="utf-8">
<meta http-equiv="Content-Security-Policy" content="default-src 'none'; img-src https: http: data:; media-src https: http: data:; style-src 'unsafe-inline' https://cdnjs.cloudflare.com; font-src https://cdnjs.cloudflare.com data:; script-src 'nonce-npp1' https://cdnjs.cloudflare.com;">
<base href="https://md.local/">
<link id="hl" rel="stylesheet">
<style>
__CSS__
</style>
<script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js"></script>
</head>
<body>
<div id="page">
<div id="file" data-head="0">
<div id="fh"><svg class="svg" viewBox="0 0 16 16" width="16" height="16" fill="currentColor" aria-hidden="true"><path d="M0 1.75A.75.75 0 0 1 .75 1h4.253c1.227 0 2.317.59 3 1.501A3.743 3.743 0 0 1 11.006 1h4.245a.75.75 0 0 1 .75.75v10.5a.75.75 0 0 1-.75.75h-4.507a2.25 2.25 0 0 0-1.591.659l-.622.621a.75.75 0 0 1-1.06 0l-.622-.621A2.25 2.25 0 0 0 5.258 13H.75a.75.75 0 0 1-.75-.75Zm7.251 10.324.004-5.073-.002-2.253A2.25 2.25 0 0 0 5.003 2.5H1.5v9h3.757a3.75 3.75 0 0 1 1.994.574ZM8.755 4.75l-.004 7.322a3.752 3.752 0 0 1 1.992-.572H14.5v-9h-3.495a2.25 2.25 0 0 0-2.25 2.25Z"></path></svg><span id="fn"></span></div>
<article id="c" class="markdown-body"></article>
</div>
</div>
<script nonce="npp1">
(function () {
  var P = {
    note: '<path d="M0 8a8 8 0 1 1 16 0A8 8 0 0 1 0 8Zm8-6.5a6.5 6.5 0 1 0 0 13 6.5 6.5 0 0 0 0-13ZM6.5 7.75A.75.75 0 0 1 7.25 7h1a.75.75 0 0 1 .75.75v2.75h.25a.75.75 0 0 1 0 1.5h-2a.75.75 0 0 1 0-1.5h.25v-2h-.25a.75.75 0 0 1-.75-.75ZM8 6a1 1 0 1 1 0-2 1 1 0 0 1 0 2Z"></path>',
    tip: '<path d="M8 1.5c-2.363 0-4 1.69-4 3.75 0 .984.424 1.625.984 2.304l.214.253c.223.264.47.556.673.848.284.411.537.896.621 1.49a.75.75 0 0 1-1.484.211c-.04-.282-.163-.547-.37-.847a8.456 8.456 0 0 0-.542-.68c-.084-.1-.173-.205-.268-.32C3.201 7.75 2.5 6.766 2.5 5.25 2.5 2.31 4.863 0 8 0s5.5 2.31 5.5 5.25c0 1.516-.701 2.5-1.328 3.259-.095.115-.184.22-.268.319-.207.245-.383.453-.541.681-.208.3-.33.565-.37.847a.751.751 0 0 1-1.485-.212c.084-.593.337-1.078.621-1.489.203-.292.45-.584.673-.848.075-.088.147-.173.213-.253.561-.679.985-1.32.985-2.304 0-2.06-1.637-3.75-4-3.75ZM5.75 12h4.5a.75.75 0 0 1 0 1.5h-4.5a.75.75 0 0 1 0-1.5ZM6 15.25a.75.75 0 0 1 .75-.75h2.5a.75.75 0 0 1 0 1.5h-2.5a.75.75 0 0 1-.75-.75Z"></path>',
    important: '<path d="M0 1.75C0 .784.784 0 1.75 0h12.5C15.216 0 16 .784 16 1.75v9.5A1.75 1.75 0 0 1 14.25 13H8.06l-2.573 2.573A1.458 1.458 0 0 1 3 14.543V13H1.75A1.75 1.75 0 0 1 0 11.25Zm1.75-.25a.25.25 0 0 0-.25.25v9.5c0 .138.112.25.25.25h2a.75.75 0 0 1 .75.75v2.19l2.72-2.72a.749.749 0 0 1 .53-.22h6.5a.25.25 0 0 0 .25-.25v-9.5a.25.25 0 0 0-.25-.25Zm7 2.25v2.5a.75.75 0 0 1-1.5 0v-2.5a.75.75 0 0 1 1.5 0ZM9 9a1 1 0 1 1-2 0 1 1 0 0 1 2 0Z"></path>',
    warning: '<path d="M6.457 1.047c.659-1.234 2.427-1.234 3.086 0l6.082 11.378A1.75 1.75 0 0 1 14.082 15H1.918a1.75 1.75 0 0 1-1.543-2.575Zm1.763.707a.25.25 0 0 0-.44 0L1.698 13.132a.25.25 0 0 0 .22.368h12.164a.25.25 0 0 0 .22-.368Zm.53 3.996v2.5a.75.75 0 0 1-1.5 0v-2.5a.75.75 0 0 1 1.5 0ZM9 11a1 1 0 1 1-2 0 1 1 0 0 1 2 0Z"></path>',
    caution: '<path d="M4.47.22A.749.749 0 0 1 5 0h6c.199 0 .389.079.53.22l4.25 4.25c.141.14.22.331.22.53v6a.749.749 0 0 1-.22.53l-4.25 4.25A.749.749 0 0 1 11 16H5a.749.749 0 0 1-.53-.22L.22 11.53A.749.749 0 0 1 0 11V5c0-.199.079-.389.22-.53Zm.84 1.28L1.5 5.31v5.38l3.81 3.81h5.38l3.81-3.81V5.31L10.69 1.5ZM8 4a.75.75 0 0 1 .75.75v3.5a.75.75 0 0 1-1.5 0v-3.5A.75.75 0 0 1 8 4Zm0 8a1 1 0 1 1 0-2 1 1 0 0 1 0 2Z"></path>',
    link: '<path d="m7.775 3.275 1.25-1.25a3.5 3.5 0 1 1 4.95 4.95l-2.5 2.5a3.5 3.5 0 0 1-4.95 0 .751.751 0 0 1 .018-1.042.751.751 0 0 1 1.042-.018 1.998 1.998 0 0 0 2.83 0l2.5-2.5a2.002 2.002 0 0 0-2.83-2.83l-1.25 1.25a.751.751 0 0 1-1.042-.018.751.751 0 0 1-.018-1.042Zm-4.69 9.64a1.998 1.998 0 0 0 2.83 0l1.25-1.25a.751.751 0 0 1 1.042.018.751.751 0 0 1 .018 1.042l-1.25 1.25a3.5 3.5 0 1 1-4.95-4.95l2.5-2.5a3.5 3.5 0 0 1 4.95 0 .751.751 0 0 1-.018 1.042.751.751 0 0 1-1.042.018 1.998 1.998 0 0 0-2.83 0l-2.5 2.5a1.998 1.998 0 0 0 0 2.83Z"></path>',
    check: '<path d="M13.78 4.22a.75.75 0 0 1 0 1.06l-7.25 7.25a.75.75 0 0 1-1.06 0L2.22 9.28a.751.751 0 0 1 .018-1.042.751.751 0 0 1 1.042-.018L6 10.94l6.72-6.72a.75.75 0 0 1 1.06 0Z"></path>',
    copy: '<path d="M0 6.75C0 5.784.784 5 1.75 5h1.5a.75.75 0 0 1 0 1.5h-1.5a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-1.5a.75.75 0 0 1 1.5 0v1.5A1.75 1.75 0 0 1 9.25 16h-7.5A1.75 1.75 0 0 1 0 14.25Z"></path><path d="M5 1.75C5 .784 5.784 0 6.75 0h7.5C15.216 0 16 .784 16 1.75v7.5A1.75 1.75 0 0 1 14.25 11h-7.5A1.75 1.75 0 0 1 5 9.25Zm1.75-.25a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-7.5a.25.25 0 0 0-.25-.25Z"></path>'
  };
  var lastHtml = null, lastName = '', keys = [];
  function svg(cls, key) {
    return '<svg class="' + cls + '" viewBox="0 0 16 16" width="16" height="16" fill="currentColor" aria-hidden="true">' + P[key] + '</svg>';
  }
  function highlight(root) {
    if (!window.hljs) return;
    root.querySelectorAll('pre code[class*="language-"]').forEach(function (e) {
      try { window.hljs.highlightElement(e); } catch (_) {}
    });
  }
  function list(root, sel) {
    return Array.prototype.slice.call(root.querySelectorAll(sel));
  }
  function decorate(root) {
    list(root, '.markdown-alert').forEach(function (a) {
      var m = /markdown-alert-(note|tip|important|warning|caution)/.exec(a.className);
      if (!m) return;
      var k = m[1];
      var q = document.createElement('blockquote');
      q.className = 'attention-header attention-' + k;
      var t = document.createElement('p');
      t.innerHTML = svg('attention-icon attention-' + k + ' svg', k) + '<strong class="attention-' + k + '">' + k.charAt(0).toUpperCase() + k.slice(1) + '</strong>';
      q.appendChild(t);
      var title = a.querySelector('.markdown-alert-title');
      Array.prototype.slice.call(a.childNodes).forEach(function (n) { if (n !== title) q.appendChild(n); });
      a.parentNode.replaceChild(q, a);
    });
    list(root, 'h1, h2, h3, h4, h5, h6').forEach(function (h) {
      if (!h.id) return;
      var a = document.createElement('a');
      a.className = 'anchor';
      a.setAttribute('href', '#' + h.id);
      a.setAttribute('aria-hidden', 'true');
      a.innerHTML = svg('svg', 'link');
      h.insertBefore(a, h.firstChild);
    });
    list(root, 'pre').forEach(function (p) {
      var w = document.createElement('div');
      w.className = 'code-block-container code-overflow-scroll';
      p.parentNode.insertBefore(w, p);
      w.appendChild(p);
      var b = document.createElement('button');
      b.type = 'button';
      b.className = 'code-copy ui button auto-hide-control';
      b.title = 'Copy';
      b.innerHTML = svg('svg', 'copy');
      w.appendChild(b);
    });
  }
  function render(top) {
    var c = document.getElementById('c');
    var y = top ? 0 : window.scrollY;
    c.innerHTML = lastHtml;
    highlight(c);
    if (document.documentElement.getAttribute('data-style') === 'forgejo') decorate(c);
    document.getElementById('fn').textContent = lastName;
    document.getElementById('file').setAttribute('data-head', lastName ? '1' : '0');
    window.scrollTo(0, y);
  }
  function goTo(id) {
    try { id = decodeURIComponent(id); } catch (_) {}
    var t = document.getElementById(id) || document.getElementsByName(id)[0];
    if (t) t.scrollIntoView();
  }
  window.goTo = goTo;
  window.setContent = function (html, name, top, anchor) {
    lastHtml = html;
    lastName = name || '';
    render(top);
    if (anchor) goTo(anchor);
  };
  window.setKeys = function (k) { keys = k; };
  var HL = {
    github: { light: 'github', dark: 'github-dark' },
    gitlab: { light: 'github', dark: 'monokai' },
    forgejo: { light: 'github', dark: 'github-dark' }
  };
  window.setLook = function (s, t) {
    if (!HL[s]) s = 'github';
    if (t !== 'light') t = 'dark';
    var h = document.documentElement;
    var changed = h.getAttribute('data-style') !== s;
    h.setAttribute('data-style', s);
    h.setAttribute('data-theme', t);
    document.getElementById('c').className = s === 'forgejo' ? 'markup file-view' : 'markdown-body';
    document.getElementById('hl').href = 'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/' + HL[s][t] + '.min.css';
    if (changed && lastHtml !== null) render();
  };
  function copy(text, btn) {
    function done() {
      btn.innerHTML = svg('svg', 'check');
      setTimeout(function () { btn.innerHTML = svg('svg', 'copy'); }, 1500);
    }
    function fallback() {
      var a = document.createElement('textarea');
      a.value = text;
      a.style.position = 'fixed';
      a.style.opacity = '0';
      document.body.appendChild(a);
      a.select();
      try { document.execCommand('copy'); } catch (_) {}
      document.body.removeChild(a);
      done();
    }
    if (navigator.clipboard && window.isSecureContext) navigator.clipboard.writeText(text).then(done, fallback);
    else fallback();
  }
  document.addEventListener('click', function (e) {
    var b = e.target.closest ? e.target.closest('.code-copy') : null;
    if (!b) return;
    var p = b.parentNode.querySelector('pre');
    if (p) copy(p.textContent, b);
  });
  document.addEventListener('click', function (e) {
    var a = e.target.closest ? e.target.closest('a[href]') : null;
    if (!a) return;
    var h = a.getAttribute('href');
    if (!h) return;
    if (h.charAt(0) === '#') {
      e.preventDefault();
      goTo(h.slice(1));
      return;
    }
    if (/^[a-z][a-z0-9+.\-]*:/i.test(h) || h.charAt(0) === '/') return;
    e.preventDefault();
    window.chrome.webview.postMessage('open:' + h);
  });
  // The web view swallows keystrokes, so the plugin shortcuts are matched here and sent to the host.
  document.addEventListener('keydown', function (e) {
    if (e.repeat) return;
    for (var i = 0; i < keys.length; i++) {
      var k = keys[i];
      if (e.keyCode === k[3] && e.ctrlKey === !!k[0] && e.altKey === !!k[1] && e.shiftKey === !!k[2]) {
        e.preventDefault();
        window.chrome.webview.postMessage(String(k[4]));
        return;
      }
    }
  }, true);
  window.setLook(document.documentElement.getAttribute('data-style'), document.documentElement.getAttribute('data-theme'));
})();
</script>
</body>
</html>
""";

        const string Css = """
html[data-style="github"] {
  --font: -apple-system, BlinkMacSystemFont, "Segoe UI", "Noto Sans", Helvetica, Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji";
  --fs: 16px; --code-fs: 85%; --radius: 6px;
  --h1: 2em; --h2: 1.5em; --h3: 1.25em; --h4: 1em; --h5: .875em; --h6: .85em;
  --hw: 600; --hb: 1px solid var(--border-muted); --hpb: .3em; --h6-color: var(--muted);
  --h1-mt: 24px; --h2-mt: 24px; --h4-mt: 24px;
  --hr-h: .25em; --hr-bg: var(--border); --hr-m: 24px 0;
  --bq-pad: 0 1em; --table-w: max-content; --pre-pad: 16px; --pre-lh: 1.45; --pre-border: 0;
}
html[data-style="github"][data-theme="dark"] {
  color-scheme: dark;
  --fg: #f0f6fc; --muted: #9198a1; --accent: #4493f8; --bg: #0d1117; --bg-muted: #151b23;
  --border: #3d444d; --border-muted: #3d444db3; --neutral-muted: #656c7633;
  --zebra: #151b23; --th-bg: transparent; --quote-border: #3d444d;
  --note: #4493f8; --tip: #3fb950; --important: #ab7df8; --warning: #d29922; --caution: #f85149;
}
html[data-style="github"][data-theme="light"] {
  color-scheme: light;
  --fg: #1f2328; --muted: #59636e; --accent: #0969da; --bg: #ffffff; --bg-muted: #f6f8fa;
  --border: #d1d9e0; --border-muted: #d1d9e0b3; --neutral-muted: #818b981f;
  --zebra: #f6f8fa; --th-bg: transparent; --quote-border: #d1d9e0;
  --note: #0969da; --tip: #1a7f37; --important: #8250df; --warning: #9a6700; --caution: #d1242f;
}
html[data-style="gitlab"] {
  --font: "GitLab Sans", -apple-system, BlinkMacSystemFont, "Segoe UI", "Noto Sans", Roboto, "Helvetica Neue", Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji";
  --fs: 14px; --code-fs: 90%; --radius: 4px;
  --h1: 28px; --h2: 24px; --h3: 20px; --h4: 16px; --h5: 14px; --h6: 12px;
  --hw: 700; --hb: 0; --hpb: 0; --h6-color: var(--fg);
  --h1-mt: 32px; --h2-mt: 24px; --h4-mt: 16px;
  --hr-h: 1px; --hr-bg: var(--border); --hr-m: 10px 0;
  --bq-pad: 8px 0 8px 16px; --table-w: max-content; --pre-pad: 12px 16px; --pre-lh: 1.6; --pre-border: 1px solid var(--border);
}
html[data-style="gitlab"][data-theme="dark"] {
  color-scheme: dark;
  --fg: #ececef; --muted: #a4a3a8; --accent: #63a6e9; --bg: #1f1e24; --bg-muted: #28272d;
  --border: #3a383f; --border-muted: #3a383f; --neutral-muted: #333238;
  --zebra: transparent; --th-bg: #28272d; --quote-border: #626168;
  --note: #63a6e9; --tip: #52b87a; --important: #a58ae8; --warning: #d99530; --caution: #f57f6c;
}
html[data-style="gitlab"][data-theme="light"] {
  color-scheme: light;
  --fg: #333238; --muted: #535158; --accent: #1068bf; --bg: #ffffff; --bg-muted: #fbfafd;
  --border: #dcdcde; --border-muted: #dcdcde; --neutral-muted: #ececef;
  --zebra: transparent; --th-bg: #fbfafd; --quote-border: #bfbfc3;
  --note: #1068bf; --tip: #108548; --important: #694cc0; --warning: #ab6100; --caution: #dd2b0e;
}
#fh { display: none; }
html[data-style="forgejo"] {
  --font: -apple-system, "Segoe UI", system-ui, Roboto, "Helvetica Neue", Arial, "Noto Sans", "Liberation Sans", sans-serif, "Apple Color Emoji", "Segoe UI Emoji";
  --mono: ui-monospace, SFMono-Regular, "SF Mono", Menlo, Monaco, Consolas, "Liberation Mono", "Courier New", monospace;
  --fs: 14px; --border-radius: 4px; --font-weight-semibold: 600; --transition-hover-fade: opacity 0.2s ease;
  --bg: var(--color-body); --fg: var(--color-text);

  #page { max-width: 1127px; margin: 0 auto; padding: 16px; }
  #fh {
    display: none; align-items: center; gap: 8px; padding: 8px 16px; font-weight: var(--font-weight-semibold);
    color: var(--color-text); background: var(--color-box-header);
    border: 1px solid var(--color-secondary); border-radius: var(--border-radius) var(--border-radius) 0 0;
  }
  #fh svg { flex-shrink: 0; }
  #file[data-head="1"] #fh { display: flex; }
  #file[data-head="1"] .file-view.markup { border-top: 0; border-top-left-radius: 0; border-top-right-radius: 0; }

  .markup {
    overflow: hidden;
    font-size: 14px;
    line-height: 1.5 !important;
    overflow-wrap: break-word;
  }
  .markup > *:first-child { margin-top: 0 !important; }
  .markup > *:last-child { margin-bottom: 0 !important; }
  .markup .msg { color: var(--color-text-light-2); text-align: center; margin-top: 48px; }
  .file-view.markup {
    padding: 1em 2em;
    font-size: 16px;
    background: var(--color-box-body);
    border: 1px solid var(--color-secondary);
    border-radius: var(--border-radius);
  }
  .markup a { color: var(--color-primary); text-decoration: none; }
  .markup a:hover { text-decoration: underline; }
  .markup a:not([href]) { color: inherit; text-decoration: none; }
  .markup b, .markup strong { font-weight: var(--font-weight-semibold); }
  .markup .anchor {
    float: left;
    padding-inline-end: 4px;
    margin-inline-start: -20px;
    color: inherit;
  }
  .markup .anchor .svg { vertical-align: middle; }
  .markup .anchor:focus { outline: none; }
  .markup h1 .anchor { margin-top: -2px; }
  .markup h1 .anchor .svg, .markup h2 .anchor .svg, .markup h3 .anchor .svg,
  .markup h4 .anchor .svg, .markup h5 .anchor .svg, .markup h6 .anchor .svg { visibility: hidden; }
  .markup h1:hover .anchor .svg, .markup h2:hover .anchor .svg, .markup h3:hover .anchor .svg,
  .markup h4:hover .anchor .svg, .markup h5:hover .anchor .svg, .markup h6:hover .anchor .svg { visibility: visible; }
  .markup h2 .anchor .svg, .markup h3 .anchor .svg, .markup h4 .anchor .svg { position: relative; top: -2px; }
  .markup h1, .markup h2, .markup h3, .markup h4, .markup h5, .markup h6 {
    margin-top: 24px;
    margin-bottom: 16px;
    font-weight: var(--font-weight-semibold);
    line-height: 1.25;
  }
  .markup h1 tt, .markup h1 code, .markup h2 tt, .markup h2 code, .markup h3 tt, .markup h3 code,
  .markup h4 tt, .markup h4 code, .markup h5 tt, .markup h5 code, .markup h6 tt, .markup h6 code {
    padding: 0 0.2em;
    font-size: inherit;
  }
  .markup h1 { padding-bottom: 0.3em; font-size: 2em; border-bottom: 1px solid var(--color-secondary); }
  .markup h2 { padding-bottom: 0.3em; font-size: 1.5em; border-bottom: 1px solid var(--color-secondary); }
  .markup h3 { font-size: 1.25em; }
  .markup h4 { font-size: 1em; }
  .markup h5 { font-size: 0.875em; }
  .markup h6 { font-size: 0.85em; color: var(--color-text-light-2); }
  .markup summary :is(h1, h2, h3, h4, h5, h6) { display: inline-block; padding-bottom: 0; border-bottom: 0; }
  .markup summary .anchor { margin-inline-start: -40px; }
  .markup summary { cursor: pointer; }
  .markup p, .markup blockquote, .markup details, .markup ul, .markup ol, .markup dl, .markup table, .markup pre {
    margin-top: 0;
    margin-bottom: 16px;
  }
  .markup p:last-child { margin-bottom: 16px; }
  .markup p { line-height: inherit; }
  .markup hr { height: 1px; padding: 0; margin: 16px 0; background-color: var(--color-secondary); border: 0; }
  .markup ul, .markup ol { padding-inline-start: 2em; }
  .markup .task-list-item { list-style-type: none; }
  .markup .task-list-item > ul { margin-top: 4px; }
  .markup .task-list-item input[type="checkbox"] {
    margin-bottom: 0.25em;
    margin-inline: -1.4em 0.6em;
    vertical-align: middle;
    padding: 0;
  }
  .markup .task-list-item + .task-list-item { margin-top: 4px; }
  .markup input[type="checkbox"] { margin-inline-end: .25em; margin-bottom: .25em; cursor: default; vertical-align: middle; }
  .markup ul ul, .markup ul ol, .markup ol ol, .markup ol ul { margin-top: 0; margin-bottom: 0; }
  .markup :is(ol, ul) ol:not([type]) { list-style-type: lower-roman; }
  .markup :is(ol, ul) :is(ol, ul) ol:not([type]) { list-style-type: lower-alpha; }
  .markup li > p { margin-top: 16px; }
  .markup li + li { margin-top: 0.25em; }
  .markup dl { padding: 0; }
  .markup dl dt { padding: 0; margin-top: 16px; font-size: 1em; font-style: italic; font-weight: var(--font-weight-semibold); }
  .markup dl dt + dt { margin-top: 0; }
  .markup dl dd { padding: 0 16px; margin-inline-start: 0; margin-bottom: 16px; }
  .markup blockquote {
    margin-inline: 0;
    padding: 0 15px;
    color: var(--color-text-light-2);
    border-left: 0.25em solid var(--color-secondary);
  }
  .markup blockquote > :first-child { margin-top: 0; }
  .markup blockquote > :last-child { margin-bottom: 0; }
  .markup .attention-header > p:first-child { display: flex; align-items: center; }
  .markup .attention-icon { margin-inline-end: 6px; }
  .markup blockquote.attention-note { border-left-color: var(--color-info-text); }
  .markup strong.attention-note, .markup svg.attention-note { color: var(--color-info-text); }
  .markup blockquote.attention-tip { border-left-color: var(--color-success-text); }
  .markup strong.attention-tip, .markup svg.attention-tip { color: var(--color-success-text); }
  .markup blockquote.attention-important { border-left-color: var(--color-priority-text); }
  .markup strong.attention-important, .markup svg.attention-important { color: var(--color-priority-text); }
  .markup blockquote.attention-warning { border-left-color: var(--color-warning-text); }
  .markup strong.attention-warning, .markup svg.attention-warning { color: var(--color-warning-text); }
  .markup blockquote.attention-caution { border-left-color: var(--color-error-text); }
  .markup strong.attention-caution, .markup svg.attention-caution { color: var(--color-error-text); }
  .markup table { display: block; width: 100%; max-width: 100%; overflow: auto; border-collapse: collapse; font-variant: tabular-nums; }
  .markup table td > :last-child { margin-bottom: 0; }
  .markup table th { font-weight: var(--font-weight-semibold); }
  .markup table th, .markup table td { padding: 6px 13px !important; border: 1px solid var(--color-secondary) !important; }
  .markup table tr { border-top: 1px solid var(--color-secondary); }
  .markup table tr:nth-child(2n) { background-color: var(--color-markup-table-row); }
  .markup img { max-width: 100%; box-sizing: initial; }
  .markup :is(h1, h2, hr) { display: flow-root; }
  .markup img[align="right"] { padding-inline-start: 20px; }
  .markup img[align="left"] { padding-inline-end: 28px; }
  .markup pre, .markup code, .markup kbd, .markup tt { font-family: var(--mono); }
  .markup code, .markup tt {
    padding: 0.2em 0.4em;
    margin: 0;
    font-size: 85%;
    white-space: break-spaces;
    background-color: var(--color-markup-code-inline);
    border-radius: var(--border-radius);
  }
  .markup code br, .markup tt br { display: none; }
  .markup pre > code { font-size: 100%; }
  .markup .code-block-container { position: relative; }
  .markup .code-block-container.code-overflow-scroll pre { overflow-x: auto; }
  .markup .code-block-container.code-overflow-scroll pre > code { white-space: pre; overflow-wrap: normal; }
  .markup pre {
    padding: 16px;
    font-size: 85%;
    line-height: 1.45;
    background-color: var(--color-markup-code-block);
    border-radius: var(--border-radius);
  }
  .markup pre code, .markup pre tt, .markup pre code.hljs {
    display: inline;
    padding: 0;
    overflow: visible;
    line-height: inherit;
    color: inherit;
    background-color: transparent;
    border: 0;
  }
  .markup pre code::before, .markup pre code::after, .markup pre tt::before, .markup pre tt::after { content: normal; }
  .markup kbd {
    display: inline-block;
    padding: 3px 5px;
    font-size: 11px;
    line-height: 10px;
    color: var(--color-text-light);
    vertical-align: middle;
    background-color: var(--color-markup-code-inline);
    border: 1px solid var(--color-secondary);
    border-radius: var(--border-radius);
    max-width: 100%;
  }
  .markup .auto-hide-control {
    position: absolute;
    z-index: 1;
    visibility: hidden;
    opacity: 0;
    transition: var(--transition-hover-fade);
  }
  .markup .code-block-container:hover .auto-hide-control { visibility: visible; opacity: 1; }
  .markup .code-copy {
    top: 8px;
    right: 6px;
    margin: 0;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: 8px;
    color: var(--color-text);
    background: var(--color-button);
    border: 1px solid var(--color-light-border);
    border-radius: var(--border-radius);
    cursor: pointer;
  }
  .markup .code-copy:hover { background: var(--color-secondary); }
  .markup .code-copy:active { background: var(--color-secondary-dark-1); }
  .markup svg.svg { fill: currentcolor; }
}
html[data-style="forgejo"][data-theme="dark"] {
  color-scheme: dark;
  --color-body: #1e1f20; --color-box-header: #1b1c1e; --color-box-body: #161718;
  --color-primary: #4183c4; --color-secondary: #3f4248; --color-secondary-dark-1: #46494f;
  --color-text: #d2d4d8; --color-text-light: #c0c2c7; --color-text-light-2: #969aa1;
  --color-button: #191a1c; --color-light-border: #f3f3f428;
  --color-markup-table-row: #f3f3f40f; --color-markup-code-block: #f3f3f412; --color-markup-code-inline: #f3f3f428;
  --color-info-text: #2f81f7; --color-success-text: #3fb950; --color-priority-text: #a371f7;
  --color-warning-text: #d29922; --color-error-text: #f85149;
}
html[data-style="forgejo"][data-theme="light"] {
  color-scheme: light;
  --color-body: #ffffff; --color-box-header: #f1f3f5; --color-box-body: #ffffff;
  --color-primary: #4183c4; --color-secondary: #d0d7de; --color-secondary-dark-1: #c7ced5;
  --color-text: #181c21; --color-text-light: #30363b; --color-text-light-2: #5b6167;
  --color-button: #f8f9fb; --color-light-border: #0000171d;
  --color-markup-table-row: #0030600a; --color-markup-code-block: #00306010; --color-markup-code-inline: #00306012;
  --color-info-text: #0969da; --color-success-text: #1a7f37; --color-priority-text: #8250df;
  --color-warning-text: #9a6700; --color-error-text: #d1242f;
}
html, body { margin: 0; padding: 0; background: var(--bg); }
body {
  color: var(--fg);
  font-family: var(--font);
  font-size: var(--fs); line-height: 1.5; word-wrap: break-word;
}
.markdown-body { box-sizing: border-box; max-width: 980px; margin: 0 auto; padding: 24px 32px; }
.markdown-body > *:first-child { margin-top: 0 !important; }
.markdown-body > *:last-child { margin-bottom: 0 !important; }
.markdown-body .msg { color: var(--muted); text-align: center; margin-top: 48px; }
.markdown-body a { color: var(--accent); text-decoration: none; }
.markdown-body a:hover { text-decoration: underline; }
.markdown-body h1, .markdown-body h2, .markdown-body h3,
.markdown-body h4, .markdown-body h5, .markdown-body h6 {
  margin-top: var(--h2-mt); margin-bottom: 16px; font-weight: var(--hw); line-height: 1.25;
}
.markdown-body h1 { margin-top: var(--h1-mt); font-size: var(--h1); padding-bottom: var(--hpb); border-bottom: var(--hb); }
.markdown-body h2 { font-size: var(--h2); padding-bottom: var(--hpb); border-bottom: var(--hb); }
.markdown-body h3 { font-size: var(--h3); }
.markdown-body h4 { margin-top: var(--h4-mt); font-size: var(--h4); }
.markdown-body h5 { margin-top: var(--h4-mt); font-size: var(--h5); }
.markdown-body h6 { margin-top: var(--h4-mt); font-size: var(--h6); color: var(--h6-color); }
.markdown-body p, .markdown-body blockquote, .markdown-body ul, .markdown-body ol,
.markdown-body dl, .markdown-body table, .markdown-body pre, .markdown-body details {
  margin-top: 0; margin-bottom: 16px;
}
.markdown-body hr { height: var(--hr-h); padding: 0; margin: var(--hr-m); background: var(--hr-bg); border: 0; }
.markdown-body blockquote { padding: var(--bq-pad); color: var(--muted); border-left: 4px solid var(--quote-border); }
.markdown-body blockquote > :first-child { margin-top: 0; }
.markdown-body blockquote > :last-child { margin-bottom: 0; }
.markdown-body ul, .markdown-body ol { padding-left: 2em; }
.markdown-body li + li { margin-top: .25em; }
.markdown-body li > p { margin-top: 16px; }
.markdown-body ul ul, .markdown-body ul ol, .markdown-body ol ol, .markdown-body ol ul { margin-top: 0; margin-bottom: 0; }
.markdown-body dt { margin-top: 16px; font-style: italic; font-weight: 600; }
.markdown-body dd { padding: 0 16px; margin-bottom: 16px; margin-left: 0; }
.markdown-body table { display: block; width: var(--table-w); max-width: 100%; overflow: auto; border-spacing: 0; border-collapse: collapse; }
.markdown-body th, .markdown-body td { padding: 6px 13px; border: 1px solid var(--border); }
.markdown-body th { font-weight: 600; background: var(--th-bg); }
.markdown-body tr { background: var(--bg); border-top: 1px solid var(--border-muted); }
.markdown-body tr:nth-child(2n) { background: var(--zebra); }
.markdown-body img { max-width: 100%; box-sizing: content-box; }
.markdown-body code, .markdown-body tt {
  padding: .2em .4em; margin: 0; font-size: var(--code-fs); white-space: break-spaces;
  background: var(--neutral-muted); border-radius: var(--radius);
  font-family: ui-monospace, SFMono-Regular, "SF Mono", Menlo, Consolas, "Liberation Mono", monospace;
}
.markdown-body pre {
  padding: var(--pre-pad); overflow: auto; font-size: var(--code-fs); line-height: var(--pre-lh);
  background: var(--bg-muted); border: var(--pre-border); border-radius: var(--radius); word-wrap: normal;
}
.markdown-body pre code, .markdown-body pre code.hljs {
  display: inline; padding: 0; margin: 0; overflow: visible; font-size: 100%; line-height: inherit;
  word-break: normal; white-space: pre; background: transparent; border: 0; color: inherit;
}
.markdown-body kbd {
  display: inline-block; padding: 3px 5px; font: 11px ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
  line-height: 10px; color: var(--fg); vertical-align: middle; background: var(--bg-muted);
  border: 1px solid var(--border-muted); border-radius: 6px; box-shadow: inset 0 -1px 0 var(--border-muted);
}
.markdown-body summary { cursor: pointer; }
.markdown-body .task-list-item { list-style-type: none; }
.markdown-body .task-list-item input { margin: 0 .2em .25em -1.4em; vertical-align: middle; }
.markdown-body del { color: var(--muted); }
.markdown-body sup, .markdown-body sub { line-height: 0; }
.markdown-body .footnotes { margin-top: 24px; padding-top: 16px; border-top: 1px solid var(--border); font-size: 12px; color: var(--muted); }
.markdown-body .footnotes ol { padding-left: 1.5em; }
.markdown-body .markdown-alert {
  padding: .5rem 1rem; margin-bottom: 16px; color: inherit; border-left: .25em solid var(--border);
}
.markdown-body .markdown-alert > :first-child { margin-top: 0; }
.markdown-body .markdown-alert > :last-child { margin-bottom: 0; }
.markdown-body .markdown-alert .markdown-alert-title { font-weight: 500; line-height: 1; margin-bottom: 8px; }
.markdown-body .markdown-alert-note { border-left-color: var(--note); }
.markdown-body .markdown-alert-note .markdown-alert-title { color: var(--note); }
.markdown-body .markdown-alert-tip { border-left-color: var(--tip); }
.markdown-body .markdown-alert-tip .markdown-alert-title { color: var(--tip); }
.markdown-body .markdown-alert-important { border-left-color: var(--important); }
.markdown-body .markdown-alert-important .markdown-alert-title { color: var(--important); }
.markdown-body .markdown-alert-warning { border-left-color: var(--warning); }
.markdown-body .markdown-alert-warning .markdown-alert-title { color: var(--warning); }
.markdown-body .markdown-alert-caution { border-left-color: var(--caution); }
.markdown-body .markdown-alert-caution .markdown-alert-title { color: var(--caution); }
""";
    }
}
