@echo off

C:
CD C:\Demo_RedactionGame

SET GAME_TYPE=Test

ECHO.
ECHO Deleting all temporary demo files...
ECHO.
rmdir Input /s /q
mkdir Input
copy C:\Demo_RedactionGame\DemoFiles\PracticeImages\*.* Input
copy C:\Demo_RedactionGame\DemoFiles\Input_%GAME_TYPE%\*.* Input

del Output\*.* /s /q

rmdir Stats /s /q
mkdir Stats
copy C:\Demo_RedactionGame\DemoFiles\ExpectedVOAs_%GAME_TYPE%\*.* Stats

rem Setup batch file to detach the DB
echo sp_detach_db Demo_RedactionGame >"%~dp0SQL.sql"
rem Detach the Demo_RedactionGame database
sqlcmd -i "%~dp0SQL.sql"

rem Delete the files for the database
del "%~dp0\DemoFiles\DB\Demo_RedactionGame*.*"

if NOT EXIST "%~dp0\DemoFiles\DB" MKDIR "%~dp0\DemoFiles\DB"

rem Copy a blank database to the db location
copy "%~dp0\DemoFiles\BlankDB\Demo_RedactionGame*.*" "%~dp0\DemoFiles\DB\*.*"

rem Create batch file that attaches the blank db
echo CREATE DATABASE Demo_RedactionGame ON (FILENAME = "%~dp0\DemoFiles\DB\Demo_RedactionGame.mdf"), (FILENAME = "%~dp0DemoFiles\DB\Demo_RedactionGame_Log.ldf") FOR ATTACH >"%~dp0SQL.sql"
sqlcmd -i "%~dp0SQL.sql" 

rem temporary SQL file
del sql.sql

ECHO.
ECHO Done.

RunDemo.bat
EXIT
