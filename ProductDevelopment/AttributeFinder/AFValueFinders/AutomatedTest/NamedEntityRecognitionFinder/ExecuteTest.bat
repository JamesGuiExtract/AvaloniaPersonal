:: Clean Source folder
call Clean.bat

:: Supply Numbered Files for 1 hour
START CopyNumberedFiles "..\..\..\AFCore\AutomatedTest\Images\TestImage002.tif.uss" ".\Source" 200ms -h1
START CopyNumberedFiles "..\..\..\AFCore\AutomatedTest\Images\TestImage002.tif" ".\Source" 200ms -h1

:: Execute command-line for desired test
START ProcessFiles.exe MemoryLeak.fps /s

:: Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test /el
