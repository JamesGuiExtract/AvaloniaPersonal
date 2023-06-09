@echo off

:: -----------------------------------------------------------------------------
:: Def variables
:: -----------------------------------------------------------------------------

if defined programfiles(x86) set programfiles=%programfiles(x86)%
set ccdir=%programfiles%\extract systems\commoncomponents
set cd=%~dp0
set cd=%cd:~0,-1%
set initdir=%cd%\initialfiles
set workdir=%cd%\workingfiles
set dbname=Memory_Leak

:: find log file and service database directory
if defined ProgramData (
set logdir=%ProgramData%\Extract Systems\LogFiles\
set dbdir=%ProgramData%\Extract Systems\ESFAMService
) else (
set logdir=C:\Documents and Settings\All Users\Application Data\Extract Systems\LogFiles\
set dbdir=C:\Documents and Settings\All Users\Application Data\Extract Systems\ESFAMService
)

:: -----------------------------------------------------------------------------
:: Reset test
:: -----------------------------------------------------------------------------

:: replace service db
echo.Replacing Fam Service DB...
for %%i in ("%cd%\*.sdf") do copy "%%i" "%dbdir%\ESFAMService.sdf"

:: update .fps file paths for 64-bit systems
if defined programfiles(x86) "%ccdir%\sqlcompactexporter" "%dbdir%\ESFAMService.sdf" "update fpsfile set filename=replace(filename,'Program Files\','Program Files (x86)\')" ""

:: set all entries to not auto-start
"%ccdir%\sqlcompactexporter" "%dbdir%\ESFAMService.sdf" "update fpsfile set NumberOfInstances = 0" ""

:: set the first entry to auto-start
"%ccdir%\sqlcompactexporter" "%dbdir%\ESFAMService.sdf" "update fpsfile set NumberOfInstances = 1 where id in (select top (1) id from fpsfile)" ""


:: -----------------------------------------------------------------------------
:: Detach existing MEMORY_LEAK database, clear .uex directory
:: -----------------------------------------------------------------------------

:: Detach database, replace with copy from initdir
echo sp_detach_db %dbname% > "%workdir%\sql.sql"
sqlcmd -i "%workdir%\sql.sql"
del "%workdir%\sql.sql"

:: copy blank files
copy "%initdir%\%dbname%.mdf" "%workdir%"
copy "%initdir%\%dbname%_log.LDF" "%workdir%"

:: attach
echo sp_attach_db '%dbname%','%workdir%\%dbname%.mdf','%workdir%\%dbname%_log.LDF' > "%workdir%\sql.sql"
sqlcmd -i "%workdir%\sql.sql" 
del "%workdir%\sql.sql"

:: backup any existing .uex logs
if exist "%logdir%\*.uex" md "%logdir%\backup"
if exist "%logdir%\*.uex" move "%logdir%\*.uex" "%logdir%\backup\"


:: -----------------------------------------------------------------------------
:: Confirm that email and scheduled task are configured
:: -----------------------------------------------------------------------------

:: bring up email config
"%ccdir%\emailfile.exe" /c

echo.Done.
"%ccdir%\sleep" 1s
