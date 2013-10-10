IF DEFINED programfiles(x86) SET programfiles=%programfiles(x86)%

REM Clean Source folder
call Clean.bat Test_3

REM Supply 288,000 files
START CopyNumberedSets.bat OriginalImages Source 1 28

REM Execute command-line for desired test
START "%programfiles%\ProcessFiles.exe" MemoryLeak_3.fps /s

REM Start Logging Statistics to numbered subfolder
"%programfiles%\LogProcessStats.exe" ProcessFiles 5s .\Stats\Test_3 /el
