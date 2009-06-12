

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles ".\Images\Image1.tif" ".\Source" 6s -h4
START CopyNumberedFiles ".\Images\Image2.tif" ".\Source" 6s -h4

REM Execute command-line for desired test
START ProcessFiles.exe VerificationMemoryLeak4.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_4 /el
