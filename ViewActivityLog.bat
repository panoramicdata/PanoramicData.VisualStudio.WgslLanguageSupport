@echo off
echo Checking Visual Studio Experimental Instance Activity Log...
echo.

set LOG_PATH=%APPDATA%\Microsoft\VisualStudio\18.0_*Exp\ActivityLog.xml

echo Looking for activity log at: %LOG_PATH%
echo.

if exist %LOG_PATH% (
    echo Opening activity log...
    start notepad %LOG_PATH%
) else (
    echo Activity log not found!
    echo.
    echo To enable activity logging, launch VS with:
    echo devenv /log /rootsuffix Exp
)

pause
