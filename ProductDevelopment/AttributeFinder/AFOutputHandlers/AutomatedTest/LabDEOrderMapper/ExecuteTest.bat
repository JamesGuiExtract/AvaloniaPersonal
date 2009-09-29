:: Clean Source folder
call Clean.bat

:: Supply Numbered Files for 4 hours
START CopyNumberedFiles "test5.txt" ".\Source" 8s -h4

:: Execute command-line for desired test
START ProcessFiles.exe MemoryLeak.fps /s

:: Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test /el
