REM Clean Source folder
call Clean.bat

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles ".\Images\Image1.tif" ".\Source" 4s -h2
START CopyNumberedFiles ".\Images\Image2.tif" ".\Source" 4s -h2

REM Execute command-line for desired test
START ProcessFiles.exe VerificationMemoryLeak3.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_3 /el
