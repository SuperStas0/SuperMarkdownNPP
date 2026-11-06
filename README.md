# SuperMarkdown++

Plugin for Notepad++
Dockable Markdown viewer in GitHub, GitLab or Forgejo style (dark by default). Updates while you type, on save, on tab switch and when the file changes on disk.

# Start
## Installation
Download `SuperMarkdown-vX.Y.Z-x64.zip` from releases and extract it into the Notepad++ installation directory (e.g. `C:\Program Files (x86)\Notepad++`). It contains `plugins\SuperMarkdown++\`. Restart Notepad++.

## Usage
Plugins > SuperMarkdown++:

| Item | Action |
| --- | --- |
| Toggle SuperMarkdown++ | Show/hide the panel (`Ctrl+Shift+M`, changeable in Settings > Shortcut Mapper > Plugin commands) |
| Toggle Light/Dark Theme | Switch between dark and light |
| GitHub Style / GitLab Style / Forgejo Style | Switch the rendering style (the active one is checked) |

Theme and style apply instantly and are remembered in `%LOCALAPPDATA%\SuperMarkdown++\` (`theme.txt`, `style.txt`).

## Styles
| Style | Notes |
| --- | --- |
| GitHub | Default |
| GitLab | GitLab-like fonts, headings, tables, code blocks |
| Forgejo | Forgejo/Gitea-like colors and markup |

GitLab and Forgejo are approximations built from their public stylesheets. They are not pixel-exact, and the Forgejo colors are taken from Gitea's themes.

## How it loads
`SuperMarkdown++.dll` (C, `shim\shim.c`) is what Notepad++ loads. It starts .NET via hostfxr and calls
`SuperMarkdown.dll`. Startup problems show a message box.

## Notes
- Code highlighting loads highlight.js from cdnjs; offline, code blocks render unhighlighted.
- Relative images resolve against the file's folder. External links open in the default browser.
- Only `.md/.markdown/.mdown/.mkd/.mkdn/.mdx/.rmd` and unsaved tabs are rendered. On other files the panel hides itself and returns on the next Markdown tab.

# Building
## Requirements
- Notepad++ 64-bit
- .NET 8 SDK (`global.json` pins 8.0.x) and the .NET 8 **Desktop Runtime** (x64)
- 64-bit MinGW-w64 `gcc` on PATH, e.g. w64devkit (`gcc -dumpmachine` prints `x86_64-...`). If it is not on PATH, `build.cmd` also looks in `%W64DEVKIT%`, `C:\w64devkit` and your user folders.
- Microsoft Edge WebView2 Runtime (preinstalled on current Windows)

## Build and release
```
build.cmd          (build only)
release.bat        (build + create release\SuperMarkdown-vX.Y.Z-x64.zip and release\SHA256SUMS.txt)
```
Build output: `bin\Release\net8.0-windows\`. The version comes from `<Version>` in `SuperMarkdown++.csproj`. The zip contains `plugins\SuperMarkdown++\` (folder name must equal the DLL name, `SuperMarkdown++`).

## CI
Pushing a tag `vX.Y.Z` (must equal `<Version>`) builds and publishes a release with the zip:
- GitHub: `.github/workflows/build.yml` (every push/PR) and `release.yml` (tags)
- GitLab: `.gitlab-ci.yml` (needs a Windows runner)
