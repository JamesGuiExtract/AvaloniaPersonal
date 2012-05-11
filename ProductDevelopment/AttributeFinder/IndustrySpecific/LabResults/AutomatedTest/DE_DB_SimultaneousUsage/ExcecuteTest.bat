@echo off
ECHO.
ECHO Deleting all temporary demo files...
ECHO.
del images\*.* /s /q

if defined programfiles(x86) set programfiles=%programfiles(x86)%

ECHO Populating images folder...
REM Make 100 copies
START "" "i:\Common\Engineering\Tools\Utils\CopyNumberedFiles\CopyNumberedFiles.exe" .\Initial\Image1.tif  .\Input 10ms -n100

copy "%~dp0Initial\OrderMappingDB.sdf" "%~dp0DBSolution\Database Files\"

ECHO Initializing database...
rem Setup batch file to detach the DB
echo sp_detach_db DE_DB_SimultaneousUsage >"%~dp0SQL.sql"
rem Detach the DE_DB_SimultaneousUsage database
sqlcmd -i "%~dp0SQL.sql"

if NOT EXIST "%~dp0DB" MKDIR "%~dp0DB"

rem Delete the files for the database
del "%~dp0DB\*.*" /s /q

rem Copy a blank database to the db location
copy "%~dp0Initial\DE_DB_SimultaneousUsage*.*" "%~dp0DB\*.*"

rem Create batch file that attaches the blank db
echo CREATE DATABASE DE_DB_SimultaneousUsage ON (FILENAME = "%~dp0DB\DE_DB_SimultaneousUsage.mdf"), (FILENAME = "%~dp0DB\DE_DB_SimultaneousUsage_Log.ldf") FOR ATTACH >"%~dp0SQL.sql"
sqlcmd -i "%~dp0SQL.sql" 

rem temporary SQL file
del sql.sql /s /q

ECHO Starting test...
Sleep 3

START "" "%programfiles(x86)%\Extract Systems\CommonComponents\ProcessFiles.exe" Process.fps /s

START "" "%programfiles(x86)%\Extract Systems\CommonComponents\SQLCDBEditor.exe" "%~dp0Solution\Database Files\OrderMappingDB.sdf"

START "" AutoHotkeyScript.ahk