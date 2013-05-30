REM Clean Source folder
call Clean.bat

REM Supply numbered file sets every second for 4 hours
START CopyNumberedSets SourceImages Source 1 4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles.exe 5s .\Stats\Test_1 /el
