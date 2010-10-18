REM Clean Source and Destination folders
call Clean.bat

REM Supply Numbered Files for 6 hours
START CopyNumberedFiles "..\..\..\..\..\..\ProductDevelopment\AttributeFinder\AFCore\AutomatedTest\Images\Image2.tif" ".\Source" 2 -h6

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_1.fps /s

REM Start Logging Statistics every minute to a numbered subfolder
LogProcessStats.exe ProcessFiles 1m .\Stats\Test_1 /el
