@echo off

:: -----------------------------------------------------------------------------
:: Def variables
:: -----------------------------------------------------------------------------

if defined programfiles(x86) set programfiles=%programfiles(x86)%
set ccdir=%programfiles%\extract systems\commoncomponents
set cd=%~dp0
set cd=%cd:~0,-1%
set initdir=%cd%\initialfiles

:: -----------------------------------------------------------------------------
:: Reset test
:: -----------------------------------------------------------------------------

:: replace service db
echo.Replacing Fam Service DB...
copy "%initdir%\ESFAMService.sdf" "%ccdir%"

:: add queuing file to service db
echo.Adding Queuing FAM to Service DB...
"%ccdir%\sqlcompactexporter" "%ccdir%\ESFAMService.sdf" "insert into FPSFile (AutoStart, FileName) values ('true', '%cd%\q.fps')" "" > nul

echo.Done.
"%ccdir%\sleep" 1s
