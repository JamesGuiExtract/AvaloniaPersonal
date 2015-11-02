REM Clean Source folder
call Clean.bat

REM Supply numbered file sets every second for 4 hours
START "" "C:\Program Files (x86)\Extract Systems\CommonComponents\CopyNumberedFiles.exe" "..\..\..\..\..\..\ProductDevelopment\AttributeFinder\AFCore\AutomatedTest\Images\Image1.tif" ".\Source" 1s -h4

REM Execute command-line for desired test
START "" "C:\Program Files (x86)\Extract Systems\CommonComponents\ProcessFiles.exe" MemoryLeak_3.fps /s

REM Start Logging Statistics to numbered subfolder
"C:\Program Files (x86)\Extract Systems\CommonComponents\LogProcessStats.exe" ProcessFiles.exe 1m .\Stats\Test_3 /el
