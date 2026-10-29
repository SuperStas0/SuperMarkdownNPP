@echo off
setlocal
cd /d "%~dp0"

dotnet build -c Release || exit /b 1

set "OUT=bin\Release\net8.0-windows"
set "M="
for /f "delims=" %%i in ('gcc -dumpmachine') do set "M=%%i"
echo gcc target: %M%
echo %M% | findstr /i "x86_64" >nul || (echo ERROR: need 64-bit MinGW-w64 ^(x86_64^) gcc & exit /b 1)

gcc -shared -O2 -s -o "%OUT%\SuperMarkdown++.dll" shim\shim.c -ladvapi32 -luser32 || exit /b 1
echo Built %OUT%\SuperMarkdown++.dll
