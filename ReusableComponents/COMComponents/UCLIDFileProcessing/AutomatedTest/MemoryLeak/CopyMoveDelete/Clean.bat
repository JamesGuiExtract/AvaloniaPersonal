echo off
del /q .\Source\*.*
del /q .\Destination\*.*

REM Ensure Sample.txt is readable
attrib -R ".\Sample.txt"
