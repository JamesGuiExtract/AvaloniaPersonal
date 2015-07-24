echo off
IF EXIST .\Source (del /q /s /f .\Source\*.*) > nul
del /q /s /f Stats\%1\*

