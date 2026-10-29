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

        public static string Shell(string theme)
        {
            return Template.Replace("__CSS__", Css).Replace("__THEME__", theme == "light" ? "light" : "dark");
        }

        const string Template = """
<!DOCTYPE html>
<html data-theme="__THEME__">
<head>
<meta charset="utf-8">
<meta http-equiv="Content-Security-Policy" content="default-src 'none'; img-src https: http: data:; media-src https: http: data:; style-src 'unsafe-inline' https://cdnjs.cloudflare.com; font-src https://cdnjs.cloudflare.com data:; script-src 'nonce-npp1' https://cdnjs.cloudflare.com;">
<base href="https://md.local/">
<link id="hl-dark" rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/github-dark.min.css">
<link id="hl-light" rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/github.min.css">
<style>
__CSS__
</style>
<script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js"></script>
</head>
<body>
<article id="c" class="markdown-body"></article>
<script nonce="npp1">
(function () {
  function highlight(root) {
    if (!window.hljs) return;
    root.querySelectorAll('pre code[class*="language-"]').forEach(function (e) {
      try { window.hljs.highlightElement(e); } catch (_) {}
    });
  }
  window.setContent = function (html) {
    var c = document.getElementById('c');
    var y = window.scrollY;
    c.innerHTML = html;
    highlight(c);
    window.scrollTo(0, y);
  };
  window.setTheme = function (t) {
    document.documentElement.setAttribute('data-theme', t);
    document.getElementById('hl-dark').disabled = (t !== 'dark');
    document.getElementById('hl-light').disabled = (t === 'dark');
  };
  document.addEventListener('click', function (e) {
    var a = e.target.closest ? e.target.closest('a[href]') : null;
    if (!a) return;
    var h = a.getAttribute('href');
    if (h && h.charAt(0) === '#') {
      e.preventDefault();
      var id = h.slice(1);
      try { id = decodeURIComponent(id); } catch (_) {}
      var t = document.getElementById(id) || document.getElementsByName(id)[0];
      if (t) t.scrollIntoView();
    }
  });
  window.setTheme(document.documentElement.getAttribute('data-theme'));
})();
</script>
</body>
</html>
""";

        const string Css = """
html[data-theme="dark"] {
  color-scheme: dark;
  --fg: #f0f6fc; --muted: #9198a1; --accent: #4493f8; --bg: #0d1117; --bg-muted: #151b23;
  --border: #3d444d; --border-muted: #3d444db3; --neutral-muted: #656c7633;
  --note: #4493f8; --tip: #3fb950; --important: #ab7df8; --warning: #d29922; --caution: #f85149;
}
html[data-theme="light"] {
  color-scheme: light;
  --fg: #1f2328; --muted: #59636e; --accent: #0969da; --bg: #ffffff; --bg-muted: #f6f8fa;
  --border: #d1d9e0; --border-muted: #d1d9e0b3; --neutral-muted: #818b981f;
  --note: #0969da; --tip: #1a7f37; --important: #8250df; --warning: #9a6700; --caution: #d1242f;
}
html, body { margin: 0; padding: 0; background: var(--bg); }
body {
  color: var(--fg);
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", "Noto Sans", Helvetica, Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji";
  font-size: 16px; line-height: 1.5; word-wrap: break-word;
}
.markdown-body { box-sizing: border-box; max-width: 980px; margin: 0 auto; padding: 24px 32px; }
.markdown-body > *:first-child { margin-top: 0 !important; }
.markdown-body > *:last-child { margin-bottom: 0 !important; }
.markdown-body .msg { color: var(--muted); text-align: center; margin-top: 48px; }
.markdown-body a { color: var(--accent); text-decoration: none; }
.markdown-body a:hover { text-decoration: underline; }
.markdown-body h1, .markdown-body h2, .markdown-body h3,
.markdown-body h4, .markdown-body h5, .markdown-body h6 {
  margin-top: 24px; margin-bottom: 16px; font-weight: 600; line-height: 1.25;
}
.markdown-body h1 { font-size: 2em; padding-bottom: .3em; border-bottom: 1px solid var(--border-muted); }
.markdown-body h2 { font-size: 1.5em; padding-bottom: .3em; border-bottom: 1px solid var(--border-muted); }
.markdown-body h3 { font-size: 1.25em; }
.markdown-body h4 { font-size: 1em; }
.markdown-body h5 { font-size: .875em; }
.markdown-body h6 { font-size: .85em; color: var(--muted); }
.markdown-body p, .markdown-body blockquote, .markdown-body ul, .markdown-body ol,
.markdown-body dl, .markdown-body table, .markdown-body pre, .markdown-body details {
  margin-top: 0; margin-bottom: 16px;
}
.markdown-body hr { height: .25em; padding: 0; margin: 24px 0; background: var(--border); border: 0; }
.markdown-body blockquote { padding: 0 1em; color: var(--muted); border-left: .25em solid var(--border); }
.markdown-body blockquote > :first-child { margin-top: 0; }
.markdown-body blockquote > :last-child { margin-bottom: 0; }
.markdown-body ul, .markdown-body ol { padding-left: 2em; }
.markdown-body li + li { margin-top: .25em; }
.markdown-body li > p { margin-top: 16px; }
.markdown-body ul ul, .markdown-body ul ol, .markdown-body ol ol, .markdown-body ol ul { margin-top: 0; margin-bottom: 0; }
.markdown-body dt { margin-top: 16px; font-style: italic; font-weight: 600; }
.markdown-body dd { padding: 0 16px; margin-bottom: 16px; margin-left: 0; }
.markdown-body table { display: block; width: max-content; max-width: 100%; overflow: auto; border-spacing: 0; border-collapse: collapse; }
.markdown-body th, .markdown-body td { padding: 6px 13px; border: 1px solid var(--border); }
.markdown-body th { font-weight: 600; }
.markdown-body tr { background: var(--bg); border-top: 1px solid var(--border-muted); }
.markdown-body tr:nth-child(2n) { background: var(--bg-muted); }
.markdown-body img { max-width: 100%; box-sizing: content-box; }
.markdown-body code, .markdown-body tt {
  padding: .2em .4em; margin: 0; font-size: 85%; white-space: break-spaces;
  background: var(--neutral-muted); border-radius: 6px;
  font-family: ui-monospace, SFMono-Regular, "SF Mono", Menlo, Consolas, "Liberation Mono", monospace;
}
.markdown-body pre {
  padding: 16px; overflow: auto; font-size: 85%; line-height: 1.45;
  background: var(--bg-muted); border-radius: 6px; word-wrap: normal;
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
