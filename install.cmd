@echo off
rem Run as Administrator, with Notepad++ closed. Builds first if needed.
set "SRC=%~dp0bin\Release\net8.0-windows"
set "DST=%ProgramFiles%\Notepad++\plugins\SuperMarkdown++"

if not exist "%SRC%\SuperMarkdown++.dll" call "%~dp0build.cmd"
if not exist "%SRC%\SuperMarkdown++.dll" (
  echo Build failed: "%SRC%\SuperMarkdown++.dll" was not created. See the errors above.
  exit /b 1
)
if not exist "%DST%" mkdir "%DST%"
xcopy "%SRC%\*" "%DST%\" /E /Y /I
echo Installed to "%DST%". Start Notepad++ and use Plugins ^> SuperMarkdown++.
