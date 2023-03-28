@echo off
ECHO.
ECHO Deleting all temporary demo files...
ECHO.
del images\*.* /s /q

ECHO Populating images folder...
REM Make 1000 copies
START "" "i:\Common\Engineering\Tools\Utils\CopyNumberedFiles" .\Initial\Image1.tif .\Images 10ms -n1000 

rem Setup batch file to detach the DB
echo sp_detach_db DB_Scalability >"%~dp0SQL.sql"
rem Detach the DB_Scalability database
sqlcmd -i "%~dp0SQL.sql"

if NOT EXIST "%~dp0\DB" MKDIR "%~dp0\DB"

rem Delete the files for the database
del "%~dp0\DB\*.*" /s /q

rem Copy a blank database to the db location
copy "%~dp0\Initial\DB_Scalability*.*" "%~dp0\DB\*.*"

rem Create batch file that attaches the blank db
echo CREATE DATABASE DB_Scalability ON (FILENAME = "%~dp0\DB\DB_Scalability.mdf"), (FILENAME = "%~dp0\DB\DB_Scalability_Log.ldf") FOR ATTACH >"%~dp0SQL.sql"
sqlcmd -i "%~dp0SQL.sql" 

rem temporary SQL file
del sql.sql /s /q
