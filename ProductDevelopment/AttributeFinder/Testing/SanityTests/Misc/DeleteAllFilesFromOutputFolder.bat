@echo off
ECHO WARNING!!!!  You are about to delete ALL Files in the Output directory for the Sanity Test
ECHO   if you have a currently running Sanity Test, or just feel unsure
ECHO   then close this window now. DO NOT PRESS ANY KEY as prompted below
ECHO   OTHERWISE, if this is what you intend to do... 
pause

ECHO Starting delete of ALL FILES in the Output folder
ECHO Please wait...
del \\Jake\FlexIndexTesting\SanityTests\Output /s /q
pause
