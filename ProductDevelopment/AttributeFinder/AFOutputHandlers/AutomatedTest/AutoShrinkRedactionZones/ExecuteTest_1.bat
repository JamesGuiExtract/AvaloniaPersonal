REM Clean Source folder
call Clean.bat Test_1

REM Get the correct program files directory for 32/64-bit OS
if defined programfiles(x86) set programfiles=%programfiles(x86)%

REM Set component directory
set CommonComponentsDir=%programfiles%\Extract Systems\CommonComponents

REM Supply Numbered Files for 4 hours
START "" "%CommonComponentsDir%\CopyNumberedFiles.exe" ".\Image2.tif" ".\Source" 2s -h4

REM Execute command-line for desired test
START "" "%CommonComponentsDir%\ProcessFiles.exe" MemoryLeak_1.fps /s

REM Start Logging Statistics to numbered subfolder
"%CommonComponentsDir%\LogProcessStats.exe" ProcessFiles 1m .\Stats\Test_1 /el
