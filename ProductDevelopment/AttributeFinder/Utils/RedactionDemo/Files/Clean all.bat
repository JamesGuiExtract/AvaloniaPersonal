@echo off

C:
CD %~dp0

ECHO.
ECHO Deleting all temporary demo files...
ECHO.
del input\*.voa /s /q
del input\*.uss /s /q
del input\*.txt /s /q
del output\*.* /s /q

rem Setup batch file to drop the DB
echo DROP DATABASE  Demo_IDShield >"%~dp0SQL.sql"
sqlcmd -i "%~dp0SQL.sql"

rem Create and import database
"C:\Program Files (x86)\Extract Systems\CommonComponents\DatabaseMigrationWizard.exe" /databaseserver "(local)" /databasename Demo_IDShield /path "%~dp0DemoFiles\BlankDB\DatabaseExport" /import /createdatabase

rem temporary SQL file
del sql.sql

ECHO.
ECHO Done.
pause
