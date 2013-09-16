REM Clean Source folder
call Clean.bat Test_2

REM Supply Numbered Files for 6 hours
START "" "C:\Program Files\Extract Systems\CommonComponents\CopyNumberedFiles.exe" ".\Image1.tif" ".\Source" 500ms -h6

REM Execute command-line for desired test
START "" "C:\Program Files\Extract Systems\CommonComponents\ProcessFiles.exe" MemoryLeak_2.fps /s

REM Start Logging Statistics every minute to a numbered subfolder
"C:\Program Files\Extract Systems\CommonComponents\LogProcessStats.exe" ProcessFiles 1m .\Stats\Test_2 /el
