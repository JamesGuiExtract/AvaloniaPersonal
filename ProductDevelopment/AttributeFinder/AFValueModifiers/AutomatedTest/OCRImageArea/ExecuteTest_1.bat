REM Clean Source folder
call Clean.bat

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles "00034226.002.tif" ".\Source" 15s -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_1.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles SSOCR2 5s .\Stats\Test_1 /el
