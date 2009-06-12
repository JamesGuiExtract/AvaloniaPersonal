REM Clean Source folder
call Clean.bat

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles ".\..\..\AFSplittersTest\TestFiles\AS_StateCodes.txt" ".\Source" 10s -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_3 /el
