@echo off
::Set up PATH
if defined programfiles(x86) set programfiles=%programfiles(x86)%

set path=path;%programfiles%\extract systems\commoncomponents

::Clean Source folder
call Clean.bat Test_1

::Supply Numbered Files for 4 hours
START CopyNumberedFiles "..\..\..\AFCore\AutomatedTest\Images\Image1.tif.uss" ".\Source" 150ms -h4

::Execute command-line for desired test
START ProcessFiles.exe MemoryLeak.fps /s

::Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles.exe 5s .\Stats\Test_1 /el
