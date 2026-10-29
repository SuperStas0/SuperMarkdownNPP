@echo off
rem Run as Administrator, with Notepad++ closed. Build first: dotnet build -c Release
set "SRC=%~dp0bin\Release\net8.0-windows"
set "DST=%ProgramFiles%\Notepad++\plugins\MarkdownPreview"

if not exist "%SRC%\MarkdownPreview.dll" (
  echo MarkdownPreview.dll not found in "%SRC%". Run: dotnet build -c Release
  exit /b 1
)
if not exist "%DST%" mkdir "%DST%"
xcopy "%SRC%\*" "%DST%\" /E /Y /I
echo Installed to "%DST%". Start Notepad++ and use Plugins ^> Markdown Preview.
