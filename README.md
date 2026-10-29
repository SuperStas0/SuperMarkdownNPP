# Markdown Preview (Notepad++ plugin)

Dockable GitHub-style Markdown preview (dark by default). Updates while you type, on save, on tab switch and when the file changes on disk.

## Requirements
- Notepad++ 64-bit (check `?` > About)
- Visual Studio Build Tools with the **Desktop development with C++** workload (DNNE compiles the native export DLL)
- .NET 8 SDK (`global.json` pins 8.0.x) and the .NET 8 **Desktop Runtime** (x64)
- Microsoft Edge WebView2 Runtime (preinstalled on current Windows)

## Build and install
```
dotnet build -c Release
install.cmd        (as Administrator, Notepad++ closed)
```
Output: `bin\Release\net8.0-windows\`. Manual install = copy that whole folder to
`C:\Program Files\Notepad++\plugins\MarkdownPreview\` (folder name must equal `MarkdownPreview`).

## Use
Plugins > Markdown Preview > Toggle Markdown Preview. Theme toggle is in the same menu (remembered).

## Notes
- Code highlighting loads highlight.js from cdnjs; offline, code blocks render unhighlighted.
- Relative images resolve against the file's folder. External links open in the default browser.
- Only `.md/.markdown/.mdown/.mkd/.mkdn/.mdx/.rmd` and unsaved tabs are rendered.
