:: -----------------------------------------------------------------------------
:: Variables
:: -----------------------------------------------------------------------------

if defined programfiles(x86) set programfiles=%programfiles(x86)%
set ccdir=%programfiles%\extract systems\commoncomponents
set cd=%~dp0
set cd=%cd:~0,-1%
set initdir=%cd%\initialfiles
set workdir=%cd%\workingfiles
set dbname=MEMORY_LEAK

:: find log file and service database directory
if defined ProgramData (
set logdir=%ProgramData%\Extract Systems\LogFiles\Misc
set dbdir=%ProgramData%\Extract Systems\ESFAMService
) else (
set logdir=C:\Documents and Settings\All Users\Application Data\Extract Systems\LogFiles\Misc
set dbdir=C:\Documents and Settings\All Users\Application Data\Extract Systems\ESFAMService
)

:: Write .fps file path to tmp file, save value to variable, delete tmp file
"%ccdir%\sqlcompactexporter" "%dbdir%\ESFAMService.sdf" "select top(1) filename from fpsfile" "%cd%\tmp.txt"
set /p fpspath=<"%cd%\tmp.txt"
del "%cd%\tmp.txt"

:: Get relevant names and paths from .fps file path
for %%i in ("%fpspath%") do set testname=%%~ni
for %%i in ("%fpspath%") do set testpath=%%~dpi
set testpath=%testpath:~0,-1%
for %%i in ("%testpath%") do set testobject=%%~ni
set testdir=%cd%\results\%testobject%\%testname%

:: Processing time (in hours)
set processingtime=6

:: Email address to send reports
set recipients=joanna_lee@extractsystems.com

:: wait a min
"%ccdir%\sleep" 1m


:: -----------------------------------------------------------------------------
:: Clear old results/database, then start images copying and stats logging
:: -----------------------------------------------------------------------------

if exist "%testdir%\Stats\*.*" del /q "%testdir%\Stats\*.*"
if exist "%testdir%\DatabaseFiles\*.*" del /q "%testdir%\DatabaseFiles\*.*"
if exist "%testdir%\UEX_Logs\*.*" del /q "%testdir%\UEX_Logs\*.*"

call "%testpath%\%testname%.bat"
start "LogProcessStats" "%ccdir%\LogProcessStats.exe" esfamservice.exe,famprocess.exe,ssocr2.exe,imageformatconverter.exe 10s "%testdir%\Stats"

:: -----------------------------------------------------------------------------
:: Process for specified time
:: -----------------------------------------------------------------------------

:: start the FAM service
net start ESFamService

:: wait until processing time has elapsed
"%ccdir%\sleep" %processingtime%h

:: stop the FAM service
net stop ESFamService

:: wait 5 mins
"%ccdir%\sleep" 5m


:: -----------------------------------------------------------------------------
:: Backup database, .uex log(s) (stats are already logged to appropriate dir)
:: -----------------------------------------------------------------------------

:: email summary report
:: DOES NOT WORK IN CURRENT INTERNAL BUILD (9.0.0.1)
"%ccdir%\ReportViewer.exe" (local) MEMORY_LEAK "Summary of Actions and Associated Document Counts" /mailto %recipients% /subject "%testobject% - %testname% has completed"

:: email testdetails.bat
call "%cd%\testdetails.bat"
"%ccdir%\EmailFile.exe" %recipients% "%workdir%\testdetails.txt" /subject "%testobject% - %testname% testdetails.txt" 
del "%workdir%\testdetails.txt"

:: wait another minute
"%ccdir%\sleep" 1m

:: detach database
echo sp_detach_db %dbname% > "%workdir%\sql.sql"
sqlcmd -i "%workdir%\sql.sql"
del "%workdir%\sql.sql"

:: copy database files
md "%testdir%\DatabaseFiles"
if exist "%workdir%\%dbname%.mdf" move "%workdir%\%dbname%.mdf" "%testdir%\DatabaseFiles"
if exist "%workdir%\%dbname%_log.LDF" move "%workdir%\%dbname%_log.LDF" "%testdir%\DatabaseFiles"

:: copy blank files
copy "%initdir%\%dbname%.mdf" "%workdir%"
copy "%initdir%\%dbname%_log.LDF" "%workdir%"

:: attach
echo sp_attach_db '%dbname%','%workdir%\%dbname%.mdf','%workdir%\%dbname%_log.LDF' > "%workdir%\sql.sql"
sqlcmd -i "%workdir%\sql.sql" 
del "%workdir%\sql.sql"

:: copy .uex logs
md "%testdir%\UEX_Logs"
if exist "%logdir%\*.uex" move "%logdir%\*.uex" "%testdir%\UEX_Logs"

:: -----------------------------------------------------------------------------
:: Set up next iteration of the test
:: -----------------------------------------------------------------------------

:: remove an entry from the service DB
"%ccdir%\sqlcompactexporter" "%dbdir%\ESFAMService.sdf" "delete fpsfile where id in (select top (1) id from fpsfile)" ""

:: set the first remaining entry to auto-start
"%ccdir%\sqlcompactexporter" "%dbdir%\ESFAMService.sdf" "update fpsfile set autostart = 'true' where id in (select top (1) id from fpsfile)" ""

:: write out temp file if there is another .fps file in the database
"%ccdir%\sqlcompactexporter" "%dbdir%\ESFAMService.sdf" "select top(1) filename from fpsfile" "%cd%\continue.txt"

:: wait a min
"%ccdir%\sleep" 1m

:: if continue.txt exists, delete it and reboot
if exist "%cd%\continue.txt" (
del "%cd%\continue.txt"
shutdown -r -t 0
)

