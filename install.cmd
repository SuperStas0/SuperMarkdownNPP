@echo off
rem Run as Administrator, with Notepad++ closed. Build first: build.cmd
set "SRC=%~dp0bin\Release\net8.0-windows"
set "DST=%ProgramFiles%\Notepad++\plugins\SuperMarkdown++"

if not exist "%SRC%\SuperMarkdown++.dll" (
  echo SuperMarkdown++.dll not found in "%SRC%". Run build.cmd first.
  exit /b 1
)
if not exist "%DST%" mkdir "%DST%"
xcopy "%SRC%\*" "%DST%\" /E /Y /I
echo Installed to "%DST%". Start Notepad++ and use Plugins ^> SuperMarkdown++.
