REM Clean Source folder
call Clean.bat Test_7

REM Supply Numbered Files for 6 hours - first VOA files, then image files
START CopyNumberedFiles.exe "samplecolor.pdf.voa" ".\Source" 1s -h6
START CopyNumberedFiles.exe "samplecolor.pdf" ".\Source" 1s -h6

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_7.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 1m .\Stats\Test_7 /el
