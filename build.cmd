@echo off
setlocal
cd /d "%~dp0"

dotnet build -c Release || exit /b 1

rem --- locate gcc (w64devkit is portable and usually not on PATH) ---
where gcc >nul 2>&1 && goto havegcc
for %%D in ("%W64DEVKIT%" "C:\w64devkit" "%USERPROFILE%\w64devkit" "%USERPROFILE%\Downloads\w64devkit" "%USERPROFILE%\Desktop\w64devkit") do if exist "%%~D\bin\gcc.exe" set "PATH=%%~D\bin;%PATH%"
where gcc >nul 2>&1 && goto havegcc
echo.
echo ERROR: gcc not found. Point this script at your w64devkit folder first, e.g.:
echo     set W64DEVKIT=D:\tools\w64devkit
echo     build.cmd
echo ^(the folder that contains w64devkit.exe and bin\gcc.exe^)
exit /b 1

:havegcc
set "OUT=bin\Release\net8.0-windows"
set "M="
for /f "delims=" %%i in ('gcc -dumpmachine') do set "M=%%i"
echo gcc target: %M%
echo %M% | findstr /i "x86_64" >nul || goto bad64
gcc -shared -O2 -s -o "%OUT%\SuperMarkdown++.dll" shim\shim.c -ladvapi32 -luser32 || exit /b 1
echo Built %OUT%\SuperMarkdown++.dll
exit /b 0

:bad64
echo ERROR: gcc is not x86_64 ^(got %M%^).
exit /b 1
