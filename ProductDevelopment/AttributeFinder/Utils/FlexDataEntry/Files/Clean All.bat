@echo off
ECHO.
ECHO Deleting all temporary demo files...
ECHO.
del input\*.voa /s /q
del input\*.uss /s /q
del input\*.xml /s /q

rem Setup batch file to detach the DB
echo sp_detach_db Demo_FlexIndex >"%~dp0SQL.sql"
rem Detach the Demo_FlexIndex database
sqlcmd -i "%~dp0SQL.sql"

rem Delete the files for the database
del "%~dp0\DemoFiles\DB\Demo_FlexIndex*.*"

if NOT EXIST "%~dp0\DemoFiles\DB" MKDIR "%~dp0\DemoFiles\DB"

rem Copy a blank database to the db location
copy "%~dp0\DemoFiles\BlankDB\Demo_FlexIndex*.*" "%~dp0\DemoFiles\DB\*.*"

rem Create batch file that attaches the blank db
echo CREATE DATABASE Demo_FlexIndex ON (FILENAME = "%~dp0\DemoFiles\DB\Demo_FlexIndex.mdf"), (FILENAME = "%~dp0DemoFiles\DB\Demo_FlexIndex_Log.ldf") FOR ATTACH >"%~dp0SQL.sql"
sqlcmd -i "%~dp0SQL.sql" 

rem temporary SQL file
del sql.sql

ECHO.
ECHO Done.
pause
