REM Clean Source folder
call Clean.bat Test_1

REM create TestArea
mkdir Source

REM Supply numbered file sets every 120 seconds for 6 hours
START CopyNumberedSets SourceImages Source 120 6

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_5.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats.exe ProcessFiles 1m .\Stats\Test_5 /el
