REM Clean Source and Destination folders
call Clean.bat

REM Supply Numbered Files every 10 seconds for 6 hours
START CopyNumberedSets images TestArea 10 6

REM Execute command-line for desired test(s)
REM START ProcessFiles.exe NoHCData.fps /s
REM START ProcessFiles.exe NoMC-LC-Warranty.fps /s
START ProcessFiles.exe VOAConditionSkipUnclassified.fps /s

REM Start Logging Statistics every minute to a numbered subfolder
LogProcessStats ProcessFiles 1m .\Stats\Test_1 /el
