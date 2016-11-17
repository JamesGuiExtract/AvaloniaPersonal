echo off
del /f /q /s .\Source\*.*
XCOPY .\Images .\Source /I /T /E
del /S /F /q .\Stats\%1\*.*
