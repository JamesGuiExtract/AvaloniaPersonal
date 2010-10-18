REM Clean Source folder
call Clean.bat Test_1 > NUL

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles "..\..\..\AFCore\AutomatedTest\Images\Image1.tif.uss" ".\Source" 400ms -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats.exe ProcessFiles 5s .\Stats\Test_1 /el
