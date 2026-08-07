@echo off
setlocal

echo ==========================================
echo       .NET PROJECT ZIP BACKUP
echo ==========================================
echo.

:: %~dp0 has trailing slash, remove it
set "ROOT=%~dp0"
set "ROOT=%ROOT:~0,-1%"

set "BACKUP=%ROOT%\Backup"
set "WORKDIR=%TEMP%\DotNetProjectBackup"

echo Source:
echo [%ROOT%]
echo.

echo Temporary:
echo [%WORKDIR%]
echo.

if not exist "%BACKUP%" mkdir "%BACKUP%"

if exist "%WORKDIR%" rmdir /s /q "%WORKDIR%"
mkdir "%WORKDIR%"

echo Copying project files...
echo.

robocopy "%ROOT%" "%WORKDIR%" /E /XD bin obj .vs .vscode .git node_modules Backup /XF *.dll *.exe *.pdb *.cache *.zip

echo.
echo Robocopy finished with code: %ERRORLEVEL%
echo.

:: Robocopy exit codes 0-7 are success
if %ERRORLEVEL% GEQ 8 (
    echo ==========================================
    echo ROBOCOPY FAILED
    echo ==========================================
    pause
    exit /b 1
)

echo Creating ZIP...
echo.

for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyy-MM-dd_HH-mm-ss"') do set "DATE=%%i"

set "ZIP=%BACKUP%\CloudStorage_%DATE%.zip"

powershell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -Path '%WORKDIR%\*' -DestinationPath '%ZIP%' -Force"

echo.
echo Cleaning temporary files...

rmdir /s /q "%WORKDIR%"

echo.

if exist "%ZIP%" (
    echo ==========================================
    echo SUCCESS!
    echo ==========================================
    echo.
    echo ZIP created:
    echo.
    echo "%ZIP%"
) else (
    echo ==========================================
    echo FAILED!
    echo ==========================================
    echo.
    echo ZIP was not created.
)

echo.
pause