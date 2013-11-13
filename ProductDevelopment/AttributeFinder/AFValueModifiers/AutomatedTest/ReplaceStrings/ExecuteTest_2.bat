REM Clean Source folder
call Clean.bat Test_1 > NUL

REM Supply Numbered Files for 4 hours
START "" "C:\Program Files (x86)\Extract Systems\CommonComponents\CopyNumberedFiles.exe" "..\..\..\AFCore\AutomatedTest\Images\TestImage002.tif.uss" ".\Source" 250ms -h4

REM Execute command-line for desired test
START "" "C:\Program Files (x86)\Extract Systems\CommonComponents\ProcessFiles.exe" MemoryLeak_2.fps /s

REM Start Logging Statistics to numbered subfolder
"C:\Program Files (x86)\Extract Systems\CommonComponents\LogProcessStats.exe" ProcessFiles 1m .\Stats\Test_2 /el
