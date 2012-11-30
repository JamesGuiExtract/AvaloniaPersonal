REM Clean Source folder
call Clean.bat Test_1

:: Wait a few seconds
"%ccdir%\sleep" 10s

REM Supply Numbered Files for 4 hours
START "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" ".\Images\Image1.tif.uss" ".\Source" 1s -h4

REM Execute command-line for desired test
START "ProcessFiles" "%ccdir%\ProcessFiles.exe" MemoryLeak_1.fps /s

REM Start Logging Statistics to numbered subfolder
"%ccdir%\LogProcessStats.exe" ProcessFiles 5s .\Stats\Test_1 /el
