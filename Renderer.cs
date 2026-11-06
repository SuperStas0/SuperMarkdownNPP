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
  var HL = {
    github: { light: 'github', dark: 'github-dark' },
    gitlab: { light: 'github', dark: 'monokai' },
    forgejo: { light: 'github', dark: 'github-dark' }
  };
  window.setLook = function (s, t) {
    if (!HL[s]) s = 'github';
    if (t !== 'light') t = 'dark';
    var h = document.documentElement;
    h.setAttribute('data-style', s);
    h.setAttribute('data-theme', t);
    document.getElementById('hl').href = 'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/' + HL[s][t] + '.min.css';
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
html[data-style="forgejo"] {
  --font: -apple-system, "Segoe UI", system-ui, Roboto, "Helvetica Neue", Arial, "Noto Sans", sans-serif, "Apple Color Emoji", "Segoe UI Emoji";
  --fs: 16px; --code-fs: 85%; --radius: 4px;
  --h1: 2em; --h2: 1.5em; --h3: 1.25em; --h4: 1em; --h5: .875em; --h6: .85em;
  --hw: 600; --hb: 1px solid var(--border); --hpb: .3em; --h6-color: var(--muted);
  --h1-mt: 24px; --h2-mt: 24px; --h4-mt: 24px;
  --hr-h: 1px; --hr-bg: var(--border); --hr-m: 16px 0;
  --bq-pad: 0 15px; --table-w: 100%; --pre-pad: 16px; --pre-lh: 1.45; --pre-border: 0;
}
html[data-style="forgejo"][data-theme="dark"] {
  color-scheme: dark;
  --fg: #d2d4d8; --muted: #969aa1; --accent: #4183c4; --bg: #161718; --bg-muted: #f3f3f412;
  --border: #3f4248; --border-muted: #3f4248; --neutral-muted: #f3f3f428;
  --zebra: #f3f3f40f; --th-bg: transparent; --quote-border: #3f4248;
  --note: #4493f8; --tip: #3fb950; --important: #ab7df8; --warning: #d29922; --caution: #f85149;
}
html[data-style="forgejo"][data-theme="light"] {
  color-scheme: light;
  --fg: #181c21; --muted: #5b6167; --accent: #4183c4; --bg: #ffffff; --bg-muted: #00306010;
  --border: #d0d7de; --border-muted: #d0d7de; --neutral-muted: #00306012;
  --zebra: #0030600a; --th-bg: transparent; --quote-border: #d0d7de;
  --note: #0969da; --tip: #1a7f37; --important: #8250df; --warning: #9a6700; --caution: #d1242f;
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
