REM Clean Source folder
call Clean.bat Test_1

:: Wait a few seconds
"%ccdir%\sleep" 10s

REM Execute command-line for desired test
START "ProcessFiles" "%ccdir%\ProcessFiles.exe" MemoryLeak_1.fps /s

REM Start Logging Statistics to numbered subfolder
START "" "%ccdir%\LogProcessStats.exe" ProcessFiles 5s .\Stats\Test_1 /el

REM Supply Numbered Files
CALL "CopyNumberedSets.bat" "." "Source" 1 100
