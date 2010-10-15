REM Clean Source folder
call Clean.bat Test_1

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles "PersonNameTests.txt" ".\Source" 1s -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_1.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_1 /el
