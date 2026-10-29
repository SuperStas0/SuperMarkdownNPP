# SuperMarkdown++ (Notepad++ plugin)

Dockable GitHub-style Markdown preview (dark by default). Updates while you type, on save, on tab switch and when the file changes on disk.

## Requirements
- Notepad++ 64-bit
- .NET 8 SDK (`global.json` pins 8.0.x) and the .NET 8 **Desktop Runtime** (x64)
- 64-bit MinGW-w64 `gcc` on PATH, e.g. w64devkit (`gcc -dumpmachine` prints `x86_64-...`)
- Microsoft Edge WebView2 Runtime (preinstalled on current Windows)

## Build and install
```
build.cmd
install.cmd        (as Administrator, Notepad++ closed)
```
Output: `bin\Release\net8.0-windows\`. Manual install = copy that whole folder to
`C:\Program Files\Notepad++\plugins\SuperMarkdown++\` (folder name must equal the DLL name, `SuperMarkdown++`).

## Use
Plugins > SuperMarkdown++ > Toggle Preview. Theme toggle is in the same menu (remembered).

## How it loads
`SuperMarkdown++.dll` (C, `shim\shim.c`) is what Notepad++ loads. It starts .NET via hostfxr and calls
`MarkdownPreviewPlugin.dll`. Startup problems show a message box.

## Notes
- Code highlighting loads highlight.js from cdnjs; offline, code blocks render unhighlighted.
- Relative images resolve against the file's folder. External links open in the default browser.
- Only `.md/.markdown/.mdown/.mkd/.mkdn/.mdx/.rmd` and unsaved tabs are rendered.
