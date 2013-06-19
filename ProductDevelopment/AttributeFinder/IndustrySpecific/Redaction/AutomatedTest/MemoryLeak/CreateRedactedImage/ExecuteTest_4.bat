REM Clean Source folder
call Clean.bat Test_4

REM Supply Numbered Files for 4 hours - first VOA files, then image files
START CopyNumberedFiles.exe "ImageWithAnnotations.tif.voa" ".\Source" 200ms -h4
START CopyNumberedFiles.exe "ImageWithAnnotations.tif" ".\Source" 200ms -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_4.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_4 /el
