@echo off
setlocal
cd /d "%~dp0"

call build.cmd || exit /b 1

set "DEST="
if defined NPP_DIR if exist "%NPP_DIR%\notepad++.exe" set "DEST=%NPP_DIR%"
if not defined DEST if exist "%ProgramFiles%\Notepad++\notepad++.exe" set "DEST=%ProgramFiles%\Notepad++"
if not defined DEST if exist "%ProgramFiles(x86)%\Notepad++\notepad++.exe" set "DEST=%ProgramFiles(x86)%\Notepad++"
if not defined DEST goto nonpp

tasklist /fi "imagename eq notepad++.exe" 2>nul | find /i "notepad++.exe" >nul && goto running

xcopy "bin\Release\net8.0-windows\*" "%DEST%\plugins\SuperMarkdown++\" /E /Y /I /Q >nul || goto copyfail
echo Installed into %DEST%\plugins\SuperMarkdown++
exit /b 0

:nonpp
echo ERROR: Notepad++ not found. Set NPP_DIR to its folder, e.g. set NPP_DIR=D:\npp
exit /b 1

:running
echo ERROR: Notepad++ is running. Close it and run again.
exit /b 1

:copyfail
echo ERROR: copy to %DEST% failed. Run as administrator if it is under Program Files.
exit /b 1
