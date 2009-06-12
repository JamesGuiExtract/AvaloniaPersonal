REM Clean Source folder
call Clean.bat

REM Supply Numbered Files for 4 hours
START CopyNumberedSets Images Source 10 4

REM Execute command-line for desired test
START ProcessFiles.exe VerificationMemoryLeak5.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_5 /el
