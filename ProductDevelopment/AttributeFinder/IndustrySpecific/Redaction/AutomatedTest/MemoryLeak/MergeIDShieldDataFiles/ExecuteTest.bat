REM Clean Source folder
call Clean.bat Test_1

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles.exe ".\Images\1.voa" ".\Source" 50ms -h4
START CopyNumberedFiles.exe ".\Images\2.voa" ".\Source" 50ms -h4
START CopyNumberedFiles.exe ".\Images\1.tif" ".\Source" 50ms -h4
START CopyNumberedFiles.exe ".\Images\2.tif" ".\Source" 50ms -h4

REM Execute command-line for desired test
START ProcessFiles.exe FilterIDShieldDataFile.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_1 /el
