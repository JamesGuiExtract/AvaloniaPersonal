:: Clean Source folder
call Clean.bat

:: Supply Numbered Files for 4 hours
START CopyNumberedFiles "..\..\..\AFCore\AutomatedTest\Images\TwoStapledIDShieldDemoImages.tif.uss" ".\Source" 200ms -h4

:: Execute command-line for desired test
START ProcessFiles.exe MemoryLeak.fps /s

:: Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test /el
