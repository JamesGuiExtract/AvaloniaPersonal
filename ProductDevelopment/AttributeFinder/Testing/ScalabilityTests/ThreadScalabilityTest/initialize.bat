@echo off

:: -----------------------------------------------------------------------------
:: Def variables
:: -----------------------------------------------------------------------------

if defined programfiles(x86) set programfiles=%programfiles(x86)%
set ccdir=%programfiles%\extract systems\commoncomponents
set cd=%~dp0
set cd=%cd:~0,-1%
set initdir=%cd%\initialfiles

:: find log file and service database directory
if defined ProgramData (
set logdir=%ProgramData%\Extract Systems\LogFiles
set dbdir=%ProgramData%\Extract Systems\ESFAMService
) else (
set logdir=C:\Documents and Settings\All Users\Application Data\Extract Systems\LogFiles
set dbdir=C:\Documents and Settings\All Users\Application Data\Extract Systems\ESFAMService
)

:: -----------------------------------------------------------------------------
:: Reset test
:: -----------------------------------------------------------------------------

:: replace service db
echo.Replacing Fam Service DB...
copy "%initdir%\ESFAMService.sdf" "%dbdir%"

:: add queuing file to service db
echo.Adding Queuing FAM to Service DB...
"%ccdir%\sqlcompactexporter" "%dbdir%\ESFAMService.sdf" "insert into FPSFile (NumberOfInstances, FileName, NumberOfFilesToProcess) values (1, '%cd%\q.fps', -1)" "" > nul

echo.Done.
"%ccdir%\sleep" 1s
