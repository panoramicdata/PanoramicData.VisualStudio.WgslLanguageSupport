@echo off
echo ========================================
echo WGSL Extension - Development Reset Script
echo ========================================
echo.
echo This script cleans the Experimental Instance cache
echo WITHOUT permanently installing the extension.
echo After running this, just press F5 to debug!
echo.

set DEVENV="C:\Program Files\Microsoft Visual Studio\18\Professional\Common7\IDE\devenv.exe"

echo Step 1: Killing any running Experimental Instance...
taskkill /F /IM devenv.exe /FI "WINDOWTITLE eq *Experimental Instance*" 2>NUL
if %ERRORLEVEL% EQU 0 (
    echo   - Closed Experimental Instance
) else (
    echo   - No Experimental Instance running
)
echo.

echo Step 2: Deleting MEF component cache...
for /d %%G in ("%LOCALAPPDATA%\Microsoft\VisualStudio\18.0_*Exp") do (
    echo Cleaning cache in %%~nxG...
    if exist "%%G\ComponentModelCache" (
        rd /s /q "%%G\ComponentModelCache"
        echo   - Deleted ComponentModelCache
    )
    if exist "%%G\Extensions\extensions.configurationchanged" (
        del /f /q "%%G\Extensions\extensions.configurationchanged"
        echo   - Deleted extensions cache marker
    )
)
echo.

echo Step 3: Clearing MEF cache via devenv...
%DEVENV% /clearcache /rootsuffix Exp
echo.

echo ========================================
echo Cache Cleared Successfully!
echo ========================================
echo.
echo Next steps:
echo 1. Go back to your main Visual Studio
echo 2. Press F5 to start debugging
echo 3. VS will automatically deploy the extension
echo 4. Open a .wgsl file in the Experimental Instance
echo 5. You should see syntax highlighting!
echo.
echo NOTE: The extension is NOT permanently installed.
echo Each time you press F5, VS redeploys the latest build.
echo.
pause
