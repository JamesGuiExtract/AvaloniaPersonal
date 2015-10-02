REM Clean Source folder
call Clean.bat Test_3 > NUL

REM Supply Numbered Files for 4 hours
START "" "C:\Program Files (x86)\Extract Systems\CommonComponents\CopyNumberedFiles.exe" ".\Test21and22.txt" ".\Source" 250ms -h4

REM Execute command-line for desired test
START "" "C:\Program Files (x86)\Extract Systems\CommonComponents\ProcessFiles.exe" MemoryLeak_3.fps /s

REM Start Logging Statistics to numbered subfolder
"C:\Program Files (x86)\Extract Systems\CommonComponents\LogProcessStats.exe" ProcessFiles 1m .\Stats\Test_3 /el
