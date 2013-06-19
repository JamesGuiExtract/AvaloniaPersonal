REM Clean Source folder
call Clean.bat Test_1

REM Supply Numbered Files for 4 hours - first VOA files, then image files
START CopyNumberedFiles.exe "Image1.tif.voa" ".\Source" 100ms -h4
START CopyNumberedFiles.exe "Image1.tif" ".\Source" 100ms -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_1.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_1 /el
