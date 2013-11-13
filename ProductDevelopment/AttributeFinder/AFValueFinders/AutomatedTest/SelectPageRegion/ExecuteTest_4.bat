REM Clean Source folder
call Clean.bat

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles "..\..\..\AFCore\AutomatedTest\Images\TestImage002.tif.uss" ".\Source" 250ms -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_4.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_4 /el
