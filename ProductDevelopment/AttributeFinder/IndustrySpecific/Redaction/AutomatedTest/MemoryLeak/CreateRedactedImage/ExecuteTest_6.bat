REM Clean Source folder
call Clean.bat Test_6

REM Supply Numbered Files for 6 hours - first VOA files, then image files
START CopyNumberedFiles.exe "Image1.pdf.voa" ".\Source" 2s -h6
START CopyNumberedFiles.exe "Image1.pdf" ".\Source" 3s -h6

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_6.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_6 /el
