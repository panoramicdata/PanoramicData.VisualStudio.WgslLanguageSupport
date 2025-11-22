@echo off
echo ========================================
echo REPAIR Experimental Instance
echo ========================================
echo.
echo This will completely reset the Experimental Instance
echo to fix the MEF Service Broker error.
echo.

set DEVENV="C:\Program Files\Microsoft Visual Studio\18\Professional\Common7\IDE\devenv.exe"

echo Step 1: Killing any running instances...
taskkill /F /IM devenv.exe /FI "WINDOWTITLE eq *Experimental Instance*" 2>NUL
echo.

echo Step 2: Completely resetting Experimental Instance...
echo This will recreate all configuration files.
%DEVENV% /resetSettings /rootsuffix Exp
echo.

echo Step 3: Rebuilding MEF cache...
%DEVENV% /updateconfiguration /rootsuffix Exp
echo.

echo ========================================
echo Experimental Instance Repaired!
echo ========================================
echo.
echo The MEF Service Broker error should now be fixed.
echo.
echo Next steps:
echo 1. Press F5 in your main Visual Studio
echo 2. The Experimental Instance should launch cleanly
echo.
pause
