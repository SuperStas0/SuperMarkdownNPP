@echo off
setlocal
cd /d "%~dp0"

call build.cmd || exit /b 1

set "SRC=bin\Release\net8.0-windows"
set "STAGE=release\stage"

if exist release rmdir /s /q release
mkdir "%STAGE%\plugins\SuperMarkdown++" || exit /b 1
xcopy "%SRC%\*" "%STAGE%\plugins\SuperMarkdown++\" /E /Y /I /Q >nul || exit /b 1
tar -a -c -f release\plugins.zip -C "%STAGE%" plugins || exit /b 1
rmdir /s /q "%STAGE%"
echo Created release\plugins.zip. Extract it into the Notepad++ installation directory.
