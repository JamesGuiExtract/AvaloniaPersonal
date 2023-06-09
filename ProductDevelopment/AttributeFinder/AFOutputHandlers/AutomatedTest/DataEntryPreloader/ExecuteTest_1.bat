REM Clean Source folder
call Clean.bat Test_1

REM Get the correct program files directory for 32/64-bit OS
if defined programfiles(x86) set programfiles=%programfiles(x86)%

REM Set component directory
set CommonComponentsDir=%programfiles%\Extract Systems\CommonComponents

REM Copy Demo_LabDE DEP
COPY C:\Demo_LabDE\Solution\Bin\*.dll .\Solution\Bin\

REM Copy Demo_LabDE OMDB
COPY "C:\Demo_LabDE\Solution\Database Files\*.sdf" ".\Solution\Database Files\"

REM Supply numbered file sets every 4 seconds for 6 hours
START "" CopyNumberedSets  Images Source 4 6

REM Execute command-line for desired test
START "" "%CommonComponentsDir%\ProcessFiles.exe" MemoryLeak_1.fps /s

REM Start Logging Statistics to numbered subfolder
"%CommonComponentsDir%\LogProcessStats.exe" ProcessFiles 1m .\Stats\Test_1 /el
