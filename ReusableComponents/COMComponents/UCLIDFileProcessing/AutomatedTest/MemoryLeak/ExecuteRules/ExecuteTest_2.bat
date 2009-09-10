REM Clean Source and Destination folders
call Clean.bat

REM Supply Numbered Files for 6 hours
START CopyNumberedFiles "..\..\..\..\..\..\ProductDevelopment\AttributeFinder\AFCore\AutomatedTest\Images\Image2.tif.uss" ".\Source" 400ms -h6

REM Execute command-line for desired test
START ProcessFiles.exe 2_From_USS_Files.fps /s

REM Start Logging Statistics every minute to a numbered subfolder
LogProcessStats.exe ProcessFiles 1m .\Stats\Test_2 /el
