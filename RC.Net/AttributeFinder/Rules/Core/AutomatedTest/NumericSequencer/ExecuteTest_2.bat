REM Clean Source folder
call Clean.bat Test_2

REM Supply Numbered Files for 4 hours
START "CopyNumberedFiles" "CopyNumberedFiles.exe" ".\contractthree.txt" ".\Source" 500ms -h4

REM Execute command-line for desired test
START "ProcessFiles" "ProcessFiles.exe" MemoryLeak_2.fps /s

REM Start Logging Statistics to numbered subfolder
"LogProcessStats.exe" ProcessFiles 5s .\Stats\Test_2 /el
