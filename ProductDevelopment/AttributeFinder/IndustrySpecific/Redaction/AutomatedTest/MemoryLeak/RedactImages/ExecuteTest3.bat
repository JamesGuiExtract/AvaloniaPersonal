REM Clean Source folder
call Clean.bat

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles.exe "Image1.tif" ".\Source" 3s -h4

REM Execute command-line for desired test
START ProcessFiles.exe AutomatedRedact3.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_3 /el
