REM Clean Source folder
call Clean.bat

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles "0001pg3.tif" ".\Source" 8s -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_2.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles SSOCR2 5s .\Stats\Test_2 /el
