@echo off
setlocal
cd /d "%~dp0"

call build.cmd || exit /b 1

set "VER="
for /f "usebackq delims=" %%v in (`powershell -NoLogo -NoProfile -Command "(Select-Xml -Path 'SuperMarkdown++.csproj' -XPath '//Version').Node.InnerText"`) do set "VER=%%v"
if not defined VER (
    echo ERROR: no ^<Version^> found in SuperMarkdown++.csproj.
    exit /b 1
)

set "SRC=bin\Release\net8.0-windows"
set "STAGE=release\stage"
set "ZIP=SuperMarkdown-v%VER%-x64.zip"

if exist release rmdir /s /q release
mkdir "%STAGE%\plugins\SuperMarkdown++" || exit /b 1
xcopy "%SRC%\*" "%STAGE%\plugins\SuperMarkdown++\" /E /Y /I /Q >nul || exit /b 1
del /s /q "%STAGE%\*.pdb" >nul 2>&1
tar -a -c -f "release\%ZIP%" -C "%STAGE%" plugins || exit /b 1
rmdir /s /q "%STAGE%"

powershell -NoLogo -NoProfile -Command "$h = (Get-FileHash -Algorithm SHA256 -LiteralPath 'release\%ZIP%').Hash.ToLower(); Set-Content -NoNewline -Encoding ascii -Path 'release\SHA256SUMS.txt' -Value ($h + '  %ZIP%' + [char]10)" || exit /b 1

echo Created release\%ZIP% and release\SHA256SUMS.txt. Extract the zip into the Notepad++ installation directory.
