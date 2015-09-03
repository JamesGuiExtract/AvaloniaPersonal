ECHO Please cconfirm that the local MEMORY_LEAK database includes actions: Test, Test2
PAUSE

REM Clean Source folder
call Clean.bat Test_6

REM Supply Numbered Files for 6 hours
START CopyNumberedFiles "..\..\..\..\..\..\ProductDevelopment\AttributeFinder\AFCore\AutomatedTest\Images\Image1.pdf" ".\Source" 500ms -h6

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_6.fps /s

REM Start Logging Statistics every minute to a numbered subfolder
LogProcessStats.exe ProcessFiles 1m .\Stats\Test_6 /el
