call config_build.bat
set LOGFILE=results-%DATETIME%.txt
set LAUNCH=start /b /wait /realtime

bin\date > %LOGFILE%
wmic CPU get NAME | more >> %LOGFILE%

echo. >> %LOGFILE%
echo. >> %LOGFILE%
echo bin\TestSineSpeed-Release_x86.exe >> %LOGFILE%
echo --------------------- >> %LOGFILE%
%LAUNCH% bin\TestSineSpeed-Release_x86.exe >> %LOGFILE%

echo. >> %LOGFILE%
echo. >> %LOGFILE%
echo bin\TestSineSpeed-Release_x64.exe >> %LOGFILE%
echo --------------------- >> %LOGFILE%
%LAUNCH% bin\TestSineSpeed-Release_x64.exe >> %LOGFILE%

echo. >> %LOGFILE%
echo. >> %LOGFILE%
echo bin\TestSineSpeedCSharp.exe >> %LOGFILE%
echo --------------------------- >> %LOGFILE%
%LAUNCH% bin\TestSineSpeedCSharp.exe >> %LOGFILE%
