REM Clean Source folder
call Clean.bat Test_3

REM Supply Numbered Files for 4 hours - first VOA files, then image files
START CopyNumberedFiles.exe "Image1.tif.voa" ".\Source" 1s -h4
START CopyNumberedFiles.exe "Image1.tif" ".\Source" 1s -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_3.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_3 /el
