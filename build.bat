@echo off
setlocal
cd /d "%~dp0"

set "ROOT=%~dp0"
set "OUT=bin\Release\net8.0-windows"
set "CLEAN="
set "INSTALL="
set "RELEASE="
set "NOBUILD="
set "HELP="
set "BAD="

:args
if "%~1"=="" goto argsdone
call :flag "%~1"
shift
goto args
:argsdone

if defined HELP goto help
if defined BAD (echo ERROR: unknown flag. Run build.bat -h for usage.& exit /b 1)
if defined NOBUILD if defined CLEAN (echo ERROR: -n cannot be combined with -c.& exit /b 1)
if defined NOBUILD if not defined INSTALL if not defined RELEASE (echo ERROR: -n needs -i and/or -r.& exit /b 1)
if defined NOBUILD goto havebuild

where gcc >nul 2>&1 && goto havegcc
for %%D in ("%W64DEVKIT%" "C:\w64devkit" "%USERPROFILE%\w64devkit" "%USERPROFILE%\Downloads\w64devkit" "%USERPROFILE%\Desktop\w64devkit") do if exist "%%~D\bin\gcc.exe" set "PATH=%%~D\bin;%PATH%"
where gcc >nul 2>&1 || (echo ERROR: gcc not found. Put w64devkit on PATH or set W64DEVKIT to its folder.& exit /b 1)
:havegcc
set "M="
for /f "delims=" %%i in ('gcc -dumpmachine') do set "M=%%i"
echo %M% | findstr /i "x86_64" >nul || (echo ERROR: gcc is not x86_64.& exit /b 1)
where dotnet >nul 2>&1 || (echo ERROR: dotnet not found. Install the .NET 8 SDK.& exit /b 1)

if defined CLEAN (
    if exist bin rmdir /s /q bin
    if exist obj rmdir /s /q obj
)

dotnet build -c Release -nologo || exit /b 1

if not exist "%OUT%\SuperMarkdown.dll" (echo ERROR: SuperMarkdown.dll was not produced.& exit /b 1)
gcc -shared -O2 -s -o "%OUT%\SuperMarkdown++.dll" shim\shim.c -ladvapi32 -luser32 || exit /b 1
echo Built %OUT%\SuperMarkdown++.dll
goto built

:havebuild
if not exist "%OUT%\SuperMarkdown.dll" (echo ERROR: no build output in %OUT%. Run build.bat without -n first.& exit /b 1)
if not exist "%OUT%\SuperMarkdown++.dll" (echo ERROR: no build output in %OUT%. Run build.bat without -n first.& exit /b 1)

:built
if defined INSTALL call :install || exit /b 1
if defined RELEASE call :release || exit /b 1
exit /b 0

:flag
set "A=%~1"
set "REST=%A:~1%"
if not "%A:~0,1%"=="-" set "REST=?"
if not defined REST set "REST=?"
if not "%REST:c=%"=="%REST%" set "CLEAN=1"
if not "%REST:i=%"=="%REST%" set "INSTALL=1"
if not "%REST:r=%"=="%REST%" set "RELEASE=1"
if not "%REST:n=%"=="%REST%" set "NOBUILD=1"
if not "%REST:h=%"=="%REST%" set "HELP=1"
if defined REST set "REST=%REST:c=%"
if defined REST set "REST=%REST:i=%"
if defined REST set "REST=%REST:r=%"
if defined REST set "REST=%REST:n=%"
if defined REST set "REST=%REST:h=%"
if defined REST set "BAD=1"
exit /b 0

:findnpp
set "DEST="
if defined NPP_DIR if exist "%NPP_DIR%\notepad++.exe" set "DEST=%NPP_DIR%"
if not defined DEST if exist "%ProgramFiles%\Notepad++\notepad++.exe" set "DEST=%ProgramFiles%\Notepad++"
if not defined DEST if exist "%ProgramFiles(x86)%\Notepad++\notepad++.exe" set "DEST=%ProgramFiles(x86)%\Notepad++"
exit /b 0

:help
call :findnpp
echo Usage: build.bat [-c] [-i] [-r] [-n] [-h]
echo   -c  clean: delete previous build output first
if defined DEST echo   -i  install: copy the build to %DEST%\plugins\SuperMarkdown++
if not defined DEST echo   -i  install: copy the build to Notepad++\plugins\SuperMarkdown++ (Notepad++ not found, set NPP_DIR)
echo   -r  release: package the build into release\
echo   -n  nobuild: skip the build, reuse existing output (needs -i and/or -r, not with -c)
echo   -h  show this help
echo Build output: %ROOT%%OUT%
echo Release:      %ROOT%release
exit /b 0

:install
call :findnpp
if not defined DEST (echo ERROR: Notepad++ not found. Set NPP_DIR to its folder.& exit /b 1)
tasklist /fi "imagename eq notepad++.exe" 2>nul | find /i "notepad++.exe" >nul && (echo ERROR: Notepad++ is running. Close it first.& exit /b 1)
xcopy "%OUT%\*" "%DEST%\plugins\SuperMarkdown++\" /E /Y /I /Q >nul || (echo ERROR: copy to Notepad++ failed. Run as administrator.& exit /b 1)
echo Installed into %DEST%\plugins\SuperMarkdown++
exit /b 0

:release
set "VER="
for /f "usebackq delims=" %%v in (`powershell -NoLogo -NoProfile -Command "(Select-Xml -Path 'SuperMarkdown++.csproj' -XPath '//Version').Node.InnerText"`) do set "VER=%%v"
if not defined VER (echo ERROR: no Version found in SuperMarkdown++.csproj.& exit /b 1)
set "STAGE=release\stage"
set "ZIP=SuperMarkdown-v%VER%-x64.zip"
if exist release rmdir /s /q release
mkdir "%STAGE%\plugins\SuperMarkdown++" || exit /b 1
xcopy "%OUT%\*" "%STAGE%\plugins\SuperMarkdown++\" /E /Y /I /Q >nul || exit /b 1
del /s /q "%STAGE%\*.pdb" >nul 2>&1
tar -a -c -f "release\%ZIP%" -C "%STAGE%" plugins || exit /b 1
rmdir /s /q "%STAGE%"
powershell -NoLogo -NoProfile -Command "$h = (Get-FileHash -Algorithm SHA256 -LiteralPath 'release\%ZIP%').Hash.ToLower(); Set-Content -NoNewline -Encoding ascii -Path 'release\SHA256SUMS.txt' -Value ($h + '  %ZIP%' + [char]10)" || exit /b 1
echo Created release\%ZIP% and release\SHA256SUMS.txt
exit /b 0
