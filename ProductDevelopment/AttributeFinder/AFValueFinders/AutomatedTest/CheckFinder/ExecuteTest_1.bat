@echo off
REM Ensure the Source folder exists
if not exist "%~dp0\Source" mkdir "%~dp0\Source\"

REM Clean Source folder
call Clean.bat Test_1

REM Supply numbered file sets every 8 seconds for 4 hours
START CopyNumberedSets Images Source 8 4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_1.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles.exe 5s .\Stats\Test_1 /el
