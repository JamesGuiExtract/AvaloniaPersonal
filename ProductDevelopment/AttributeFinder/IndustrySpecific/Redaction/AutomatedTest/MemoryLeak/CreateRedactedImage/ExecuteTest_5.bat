REM Clean Source folder
call Clean.bat Test_5

REM Supply Numbered Files for 4 hours - first VOA files, then image files
START CopyNumberedFiles.exe "Image1.pdf.voa" ".\Source" 5s -h4
START CopyNumberedFiles.exe "Image1.pdf" ".\Source" 5s -h4

REM Execute command-line for desired test
START ProcessFiles.exe CreateRedactedImage5.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_5 /el
