:: -----------------------------------------------------------------------------
:: Variables
:: -----------------------------------------------------------------------------

if defined programfiles(x86) set programfiles=%programfiles(x86)%
set ccdir=%programfiles%\extract systems\commoncomponents
set cd=%~dp0
set cd=%cd:~0,-1%
set initdir=%cd%\initialfiles
set workdir=%cd%\workingfiles
set backupdir=%cd%\backupfiles
set dbname=ThreadScalabilityTest

:: find log file and service database directory
if defined ProgramData (
set logdir=%ProgramData%\Extract Systems\LogFiles
set dbdir=%ProgramData%\Extract Systems\ESFAMService
) else (
set logdir=C:\Documents and Settings\All Users\Application Data\Extract Systems\LogFiles
set dbdir=C:\Documents and Settings\All Users\Application Data\Extract Systems\ESFAMService
)

:: Processing time
set processingtime=8h

:: Email address to send reports
set recipients=wayne_lenius@extractsystems.com

:: -----------------------------------------------------------------------------
:: Clean up
:: -----------------------------------------------------------------------------

:: wait a min
"%ccdir%\sleep" 1m

:: delete voa, uss, xml, tif
del "%cd%\input\*.voa"
del "%cd%\input\*.uss"
del "%cd%\input\*.xml"
del "%cd%\input\*.tif"

:: Clear processing db

:: detach
echo sp_detach_db ThreadScalabilityTest > "%workdir%\sql.sql"
sqlcmd -i "%workdir%\sql.sql"
del "%workdir%\sql.sql"


:: move old files to backup location

:: get current date and time for folder name
for /f "tokens=1,2,3,4 delims=/ " %%a in ('DATE /T') do set date=%%d-%%b-%%c
for /f "tokens=1,2,3 delims=: " %%a in ('TIME /T') do set time=%%a.%%b%%c

:: make dir
set dest=%backupdir%\%date%_%time%
md "%dest%"
md "%dest%\UEX_Logs"

:: move the files
if exist "%workdir%\%dbname%.mdf" move "%workdir%\%dbname%.mdf" "%dest%"
if exist "%workdir%\%dbname%_log.LDF" move "%workdir%\%dbname%_log.LDF" "%dest%"

:: copy blank files
copy "%initdir%\%dbname%.mdf" "%workdir%"
copy "%initdir%\%dbname%_log.LDF" "%workdir%"

:: attach
echo sp_attach_db '%dbname%','%workdir%\%dbname%.mdf','%workdir%\%dbname%_log.LDF' > "%workdir%\sql.sql"
sqlcmd -i "%workdir%\sql.sql" 
del "%workdir%\sql.sql"

:: -----------------------------------------------------------------------------
:: Start processing
:: Note: Assumes that q.fps, run01.fps, run02.fps, ..., runNN.fps files exist
:: Note: Assumes that initial SDF file contains entry for q.fps and run01.fps
:: Note: FPS files above look for db = ThreadScalabilityTest, action = 'a'
:: -----------------------------------------------------------------------------

:: stop the FAM service
net stop ESFamService

:: modify each line in service db to allow testing single FAM instance with 1 to N threads
:: Note: 'q.fps' will remain unchanged
:: Note: 'run01.fps' will become 'run02.fps', 'run17.fps' will become 'run18.fps', etc
"%ccdir%\sqlcompactexporter" "%dbdir%\ESFAMService.sdf" "UPDATE FPSFile SET filename = SUBSTRING(filename,1,LEN(filename)-6) + SUBSTRING('00',1,2-LEN(CONVERT(NVARCHAR,(CONVERT(INT,SUBSTRING(filename,LEN(filename)-5,2))+1)))) + CONVERT(NVARCHAR,(CONVERT(INT,SUBSTRING(filename,LEN(filename)-5,2))+1)) + SUBSTRING(filename,LEN(filename)-3,4) WHERE filename NOT LIKE '%%\q%%fps'" ""

:: copy sets of images into test directory for 6 hours
call "%cd%\copynumberedsets" "%cd%\Images" "%cd%\Input" 1 6

:: start the FAM service
net start ESFamService

:: wait until processing time has elapsed
"%ccdir%\sleep" %processingtime%

:: stop the FAM service
net stop ESFamService

:: wait 5 mins
"%ccdir%\sleep" 5m

:: copy .uex logs to backup location
if exist "%logdir%\*.uex" move "%logdir%\*.uex" "%dest%\UEX_Logs"

:: email report
"%ccdir%\reportviewer" "(local)" "%dbname%" "%initdir%\DocsProcessedOnMachines.rpt" /mailto "%recipients%" /subject "Thread Scalability"

:: reboot
shutdown -r -t 0
