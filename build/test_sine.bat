wmic CPU get NAME | more > results.txt
echo bin\TestSineSpeed.exe >> results.txt
echo --------------------- >> results.txt
bin\TestSineSpeed.exe >> results.txt
echo bin\TestSineSpeedCSharp.exe >> results.txt
echo --------------------------- >> results.txt
bin\TestSineSpeedCSharp.exe >> results.txt
