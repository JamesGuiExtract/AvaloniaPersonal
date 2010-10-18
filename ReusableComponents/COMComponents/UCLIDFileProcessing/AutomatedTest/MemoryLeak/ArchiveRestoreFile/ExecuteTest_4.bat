@echo off
REM Clean Source folder
call CleanImage.bat Test_4

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles.exe ".\1.voa" ".\Source" 200ms -h4
START CopyNumberedFiles.exe ".\1.uss" ".\Source" 200ms -h4
call WAIT.bat 2
START CopyNumberedFiles.exe ".\1.tif" ".\Source" 200ms -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_4.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_4 /el
