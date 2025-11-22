@echo off
echo ========================================
echo Launch VS Experimental Instance with Logging
echo ========================================
echo.

set DEVENV="C:\Program Files\Microsoft Visual Studio\18\Professional\Common7\IDE\devenv.exe"

echo Launching Visual Studio Experimental Instance with logging enabled...
echo This will help diagnose extension loading issues.
echo.

%DEVENV% /log /rootsuffix Exp

echo.
echo VS has closed. Check the activity log for errors.
echo Run ViewActivityLog.bat to open the log file.
pause
