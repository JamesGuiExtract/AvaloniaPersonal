REM Clean Source folder
call Clean.bat Test_1

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles.exe "..\..\..\AFCore\AutomatedTest\Images\Image1.tif.uss" ".\Source" 250ms -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_1.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats.exe ProcessFiles 1m .\Stats\Test_1 /el
