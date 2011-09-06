REM Clean Source and Destination folders
call Clean.bat

REM Supply Numbered Files for 6 hours
START CopyNumberedFiles ".\Image.tif.voa" ".\Source" 1 -h6
START CopyNumberedFiles ".\Image.tif" ".\Source" 1 -h6

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_2.fps /s

REM Start Logging Statistics every minute to a numbered subfolder
LogProcessStats ProcessFiles 1m .\Stats\Test_2 /el
