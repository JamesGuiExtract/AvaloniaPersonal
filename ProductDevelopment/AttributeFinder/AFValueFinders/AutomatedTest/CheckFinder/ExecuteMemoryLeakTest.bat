@echo off
REM Ensure the Source folder exists
if not exist "%~dp0\TestArea" mkdir "%~dp0\TestArea\"

REM Clean Source folder
call Clean.bat Test_1

REM Supply numbered file sets every 8 seconds for 4 hours
START CopyNumberedSets Images TestArea 8 4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles.exe 5s .\Stats\Test_1 /el
