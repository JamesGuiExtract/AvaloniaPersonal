REM DO NOT Clean Source and Destination folders
REM call Clean.bat

REM DO NOT Supply Numbered Files for 6 hours, files should be retained from tests 1 and 2
REM START CopyNumberedFiles "..\..\..\..\..\..\ProductDevelopment\AttributeFinder\AFCore\AutomatedTest\Images\Image2.tif" ".\Source" 400ms -h6

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_3.fps /s

REM Start Logging Statistics every 5 minutes to a numbered subfolder
LogProcessStats.exe ProcessFiles 5m .\Stats\Test_3 /el
