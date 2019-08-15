:: Clean Source folder
call Clean.bat

:: Supply Numbered Files for 4 hours
START CopyNumberedFiles "input2.txt" ".\Source" 6s -h4

:: Execute command-line for desired test
START ProcessFiles.exe MemoryLeak.fps /s

:: Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test /el
