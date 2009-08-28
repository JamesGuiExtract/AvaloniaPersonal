REM Clean Source folder
call Clean.bat

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles "DateSplitterTests.tif.uss" ".\Source" 500ms -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_4.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats.exe ProcessFiles 5s .\Stats\Test_4 /el
