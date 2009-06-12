@echo off
ECHO WARNING!!!!  You are about to delete all XML & VOA files for the Sanity Test
ECHO   if you have a currently running Sanity Test, or just feel unsure
ECHO   then close this window now. DO NOT PRESS ANY KEY as prompted below
ECHO   OTHERWISE, if this is what you intend to do... 
pause

ECHO Starting delete of VOA files in the Input\A folder
ECHO Please wait...
del \\Jake\FlexIndexTesting\SanityTests\Input\A\*.voa /s
del \\Jake\FlexIndexTesting\SanityTests\Input\A\*.xml /s
pause
