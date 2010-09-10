:: -----------------------------------------------------------------------------
:: Variables
:: -----------------------------------------------------------------------------

if defined programfiles(x86) set programfiles=%programfiles(x86)%
set ccdir=%programfiles%\extract systems\commoncomponents
set cd=%~dp0
set cd=%cd:~0,-1%
set initdir=%cd%\initialfiles
set workdir=%cd%\workingfiles
set dbname=ThreadScalabilityTest

:: Processing time
set processingtime=8h

:: Email address to send reports
set recipients=your_name@extractsystems.com

:: -----------------------------------------------------------------------------
:: Clean up
:: -----------------------------------------------------------------------------

:: wait a min
"%ccdir%\sleep" 1m

:: delete voa, uss, xml
del "%cd%\input\*.voa" "%cd%\input\*.uss" "%cd%\input\*.xml"

:: Clear processing db

:: detach
echo sp_detach_db ThreadScalabilityTest > "%workdir%\sql.sql"
sqlcmd -i "%workdir%\sql.sql"
del "%workdir%\sql.sql"

:: delete old files
if exist "%workdir%\%dbname%.mdf" del "%workdir%\%dbname%.mdf"
if exist "%workdir%\%dbname%_log.LDF" del "%workdir%\%dbname%_log.LDF"

:: copy blank files
copy "%initdir%\%dbname%.mdf" "%workdir%"
copy "%initdir%\%dbname%_log.LDF" "%workdir%"

:: attach
echo sp_attach_db '%dbname%','%workdir%\%dbname%.mdf','%workdir%\%dbname%_log.LDF' > "%workdir%\sql.sql"
sqlcmd -i "%workdir%\sql.sql" 
del "%workdir%\sql.sql"

:: -----------------------------------------------------------------------------
:: Start processing
:: -----------------------------------------------------------------------------

:: stop the FAM service
net stop ESFamService

:: add line to service db
"%ccdir%\sqlcompactexporter" "%ccdir%\ESFAMService.sdf" "insert into FPSFile (AutoStart, FileName) values ('true', '%cd%\run.fps')" ""

:: start the FAM service
net start ESFamService

:: wait until processing time has elapsed
"%ccdir%\sleep" %processingtime%

:: stop the FAM service
net stop ESFamService

:: wait 5 mins
"%ccdir%\sleep" 5m

:: email report
"%ccdir%\reportviewer" "(local)" "%dbname%" "%initdir%\DocsProcessedOnMachines.rpt" /mailto "%recipients%"

:: reboot
shutdown -r -t 0
