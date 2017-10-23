call config_build.bat
set LOGFILE=results-%DATETIME%.txt

bin\date > %LOGFILE%
wmic CPU get NAME | more >> %LOGFILE%

echo. >> %LOGFILE%
echo. >> %LOGFILE%
echo bin\TestSineSpeed-Release_x86.exe >> %LOGFILE%
echo --------------------- >> %LOGFILE%
bin\TestSineSpeed-Release_x86.exe >> %LOGFILE%

echo. >> %LOGFILE%
echo. >> %LOGFILE%
echo bin\TestSineSpeed-Release_x64.exe >> %LOGFILE%
echo --------------------- >> %LOGFILE%
bin\TestSineSpeed-Release_x64.exe >> %LOGFILE%

echo. >> %LOGFILE%
echo. >> %LOGFILE%
echo bin\TestSineSpeedCSharp.exe >> %LOGFILE%
echo --------------------------- >> %LOGFILE%
bin\TestSineSpeedCSharp.exe >> %LOGFILE%
