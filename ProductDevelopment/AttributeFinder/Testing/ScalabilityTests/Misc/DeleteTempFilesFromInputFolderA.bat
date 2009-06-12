@echo off
ECHO WARNING!!!!  You are about to delete all the VOA files for the Scalability Test
ECHO   if you have a currently running Scalability Test, or just feel unsure
ECHO   then close this window now. DO NOT PRESS ANY KEY as prompted below
ECHO   OTHERWISE, if this is what you intend to do... 
pause

ECHO Starting delete of VOA files in the Input\A folder
ECHO Please wait...
del \\Jake\FlexIndexTesting\ScalabilityTests\Test1\Input\A\*.voa /s
pause
