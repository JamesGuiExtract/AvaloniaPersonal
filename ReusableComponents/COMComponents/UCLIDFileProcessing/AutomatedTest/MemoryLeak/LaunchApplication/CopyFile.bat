echo off
rem Copy Sample.txt from Source folder to fixed location
copy /A /Y %1 C:\target.txt

REM Sleep for 1500ms to provide delay before next file
Sleep 1500
