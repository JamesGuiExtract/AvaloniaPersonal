REM Clean Source folder
call Clean.bat Test_3

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles "PersonNameTests.txt" ".\Source" 10s -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_3.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_3 /el
