REM Clean Source and Destination folders
call Clean.bat Test_5

:: Wait a few seconds
"%ccdir%\sleep" 10s

REM Supply Numbered Files for 6 hours
START "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "..\..\..\..\..\..\ProductDevelopment\AttributeFinder\AFCore\AutomatedTest\Images\Image2.tif" ".\Source" 2 -h6

REM Execute command-line for desired test
START "ProcessFiles" "%ccdir%\ProcessFiles.exe" MemoryLeak_5.fps /s

REM Start Logging Statistics every minute to a numbered subfolder
"%ccdir%\LogProcessStats.exe" ProcessFiles 1m .\Stats\Test_5 /el
