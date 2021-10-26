@echo off

C:
CD %~dp0

ECHO.
ECHO Deleting all temporary demo files...
ECHO.

del input\*.voa /s /q
del input\*.xml /s /q
del input\extended\*.voa /s /q
del input\extended\*.xml /s /q
del input\extended\*_*.tif /s /q
del input\extended\*_*.tif.uss /s /q
del "Outbound XML Messages\*.*" /s /q
del "Outbound XML Messages\Sent\*.*" /s /q
del "OnBase export\*.*" /s /q

copy "Solution\Database Files\Original\OrderMappingDB.sqlite" "Solution\Database Files\OrderMappingDB.sqlite"

rem Setup batch file to drop the DB
echo DROP DATABASE  Demo_LabDE >"%~dp0SQL.sql"
sqlcmd -i "%~dp0SQL.sql"

rem Create and import database
"C:\Program Files (x86)\Extract Systems\CommonComponents\DatabaseMigrationWizard.exe" /databaseserver "(local)" /databasename Demo_LabDE /path "%~dp0DemoFiles\BlankDB\DatabaseExport" /import /createdatabase

rem temporary SQL file
del sql.sql

ECHO.
ECHO Done.
pause
