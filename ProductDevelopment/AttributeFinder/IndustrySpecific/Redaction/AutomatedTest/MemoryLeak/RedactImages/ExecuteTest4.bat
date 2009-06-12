REM Clean Source folder
REM call Clean.bat

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles.exe "ImageWithAnnotations.tif" ".\Source" 3s -h4

REM Execute command-line for desired test
START ProcessFiles.exe AutomatedRedact4.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_4 /el
