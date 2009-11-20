REM Clean Source folder
call Clean.bat

REM Supply Numbered Files for 4 hours - first VOA files, then image files
rem START CopyNumberedFiles.exe "Image1.tif.voa" ".\Source" 1s -h4
rem START CopyNumberedFiles.exe "Image1.tif" ".\Source" 1s -h4
START D:\Engineering\binaries\debug\CopyNumberedFiles.exe "Image1.tif.voa" ".\Source" 1s -h4
START D:\Engineering\binaries\debug\CopyNumberedFiles.exe "Image1.tif" ".\Source" 1s -h4

REM Execute command-line for desired test
rem START ProcessFiles.exe CreateRedactedImage1.fps /s
START D:\Engineering\binaries\debug\ProcessFiles.exe CreateRedactedImage1.fps /s

REM Start Logging Statistics to numbered subfolder
rem LogProcessStats ProcessFiles 5s .\Stats\Test_1 /el
I:\Common\Engineering\tools\utils\LogProcessStats ProcessFiles 5s .\Stats\Test_1 /el
