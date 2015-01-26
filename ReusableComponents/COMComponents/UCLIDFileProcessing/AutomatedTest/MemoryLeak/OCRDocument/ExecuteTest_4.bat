REM Clean Source and Destination folders
call Clean.bat

REM Supply Numbered Files for 6 hours
START "" "C:\Program Files (x86)\Extract Systems\CommonComponents\CopyNumberedFiles.exe" ".\150pages.tif" ".\Source" 120 -h6

REM Execute command-line for desired test
START "" "C:\Program Files (x86)\Extract Systems\CommonComponents\ProcessFiles.exe" MemoryLeak_4.fps /s

REM Start Logging Statistics every minute to a numbered subfolder
"C:\Program Files (x86)\Extract Systems\CommonComponents\LogProcessStats.exe" ProcessFiles 1m .\Stats\Test_4 /el
