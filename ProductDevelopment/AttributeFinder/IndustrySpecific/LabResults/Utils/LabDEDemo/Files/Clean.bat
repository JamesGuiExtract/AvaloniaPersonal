@echo off

C:
CD %~dp0

ECHO.
ECHO Deleting all temporary demo files...
ECHO.

del input\*.voa /s /q
del input\*.uss /s /q
del input\*.xml /s /q
del "Outbound XML Messages\*.*" /s /q
del "Outbound XML Messages\Sent\*.*" /s /q
del "Transmitted HL7 Messages\*.*" /s /q

rem Setup batch file to detach the DB
echo sp_detach_db Demo_LabDE >"%~dp0SQL.sql"
rem Detach the Demo_LabDE database
sqlcmd -i "%~dp0SQL.sql"

rem Delete the files for the database
del "%~dp0DemoFiles\DB\Demo_LabDE*.*"

if NOT EXIST "%~dp0DemoFiles\DB" MKDIR "%~dp0DemoFiles\DB"

rem Copy a blank database to the db location
copy "%~dp0DemoFiles\BlankDB\Demo_LabDE*.*" "%~dp0DemoFiles\DB\*.*"

rem Create batch file that attaches the blank db
echo CREATE DATABASE Demo_LabDE ON (FILENAME = "%~dp0DemoFiles\DB\Demo_LabDE.mdf"), (FILENAME = "%~dp0DemoFiles\DB\Demo_LabDE_Log.ldf") FOR ATTACH >"%~dp0SQL.sql"
sqlcmd -i "%~dp0SQL.sql" 

rem temporary SQL file
del sql.sql

ECHO.
ECHO Done.
pause
