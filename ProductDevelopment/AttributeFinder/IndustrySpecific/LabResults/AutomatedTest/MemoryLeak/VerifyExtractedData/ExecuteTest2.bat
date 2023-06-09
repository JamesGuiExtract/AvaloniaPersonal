@echo off

REM Start AutoHotkey script
START "VerifyExtractedMemoryLeak2-SkipTif.ahk"

REM Clean Source folder
call Clean.bat Test_2

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles ".\Images\350.tif" ".\Source" 2s -h2

REM Execute command-line for desired test
START ProcessFiles.exe VerifyExtractedMemoryLeak.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_2 /el
