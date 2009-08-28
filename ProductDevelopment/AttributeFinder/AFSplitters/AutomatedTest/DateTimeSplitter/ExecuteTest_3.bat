REM Clean Source folder
call Clean.bat

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles "DateTime.txt" ".\Source" 400ms -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_3.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats.exe ProcessFiles 5s .\Stats\Test_3 /el
