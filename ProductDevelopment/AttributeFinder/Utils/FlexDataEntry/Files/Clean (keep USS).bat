@echo off

C:
CD %~dp0

ECHO.
ECHO Deleting all temporary demo files...
ECHO.
del input\*.voa /s /q
del input\*.xml /s /q

rem Setup batch file to Drop the DB
echo DROP DATABASE Demo_FlexIndex >"%~dp0SQL.sql"
rem Detach the Demo_FlexIndex database
sqlcmd -i "%~dp0SQL.sql"

rem Create and import database
"C:\Program Files (x86)\Extract Systems\CommonComponents\DatabaseMigrationWizard.exe" /databaseserver "(local)" /databasename Demo_FlexIndex /path "%~dp0DemoFiles\BlankDB\DatabaseExport" /import /createdatabase

rem temporary SQL file
del sql.sql

ECHO.
ECHO Done.
pause
