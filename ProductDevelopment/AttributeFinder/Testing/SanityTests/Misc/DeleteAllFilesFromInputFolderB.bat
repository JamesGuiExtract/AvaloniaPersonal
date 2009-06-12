@echo off
ECHO WARNING!!!!  You are about to delete ALL Files in the 'B' directory for the Sanity Test
ECHO   if you have a currently running Sanity Test, or just feel unsure
ECHO   then close this window now. DO NOT PRESS ANY KEY as prompted below
ECHO   OTHERWISE, if this is what you intend to do... 
pause

ECHO Starting delete of ALL FILES in the Input\B folder
ECHO Please wait...
del \\Jake\FlexIndexTesting\SanityTests\Input\B /s /q
pause
