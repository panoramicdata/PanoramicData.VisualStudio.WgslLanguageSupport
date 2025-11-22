@echo off
echo ========================================
echo NUCLEAR RESET - Experimental Instance
echo ========================================
echo.
echo WARNING: This will DELETE the entire Experimental Instance
echo and create a fresh one from scratch.
echo.
echo Press Ctrl+C to cancel, or
pause
echo.

set DEVENV="C:\Program Files\Microsoft Visual Studio\18\Professional\Common7\IDE\devenv.exe"

echo Step 1: Killing all Visual Studio instances...
taskkill /F /IM devenv.exe 2>NUL
timeout /t 2 /nobreak >NUL
echo.

echo Step 2: Deleting ENTIRE Experimental Instance folder...
for /d %%G in ("%LOCALAPPDATA%\Microsoft\VisualStudio\18.0_*Exp") do (
    echo Deleting: %%~nxG
    rd /s /q "%%G"
)
echo.

echo Step 3: Deleting roaming settings...
for /d %%G in ("%APPDATA%\Microsoft\VisualStudio\18.0_*Exp") do (
    echo Deleting: %%~nxG
    rd /s /q "%%G"
)
echo.

echo Step 4: Creating fresh Experimental Instance...
echo This will take a moment...
%DEVENV% /rootsuffix Exp /command "File.Exit"
timeout /t 5 /nobreak >NUL
echo.

echo ========================================
echo Experimental Instance Recreated!
echo ========================================
echo.
echo The Experimental Instance has been completely rebuilt.
echo All previous settings and extensions are gone.
echo.
echo Next steps:
echo 1. Press F5 in your main Visual Studio
echo 2. The extension will deploy to the fresh instance
echo 3. Test with a .wgsl file
echo.
pause
