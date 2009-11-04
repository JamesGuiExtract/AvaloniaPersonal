REM Clean Source folder
call Clean.bat Test_1

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles "..\..\..\AFCore\AutomatedTest\Images\Image1.tif.uss" ".\Source" 500ms -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_1.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_1 /el
