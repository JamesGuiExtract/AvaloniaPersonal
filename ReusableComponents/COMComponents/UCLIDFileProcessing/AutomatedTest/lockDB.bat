@ECHO OFF

:: Check for empty arguments
if (%1)==() GOTO error
if (%2)==() GOTO error

:: Get the server name and database name
set server=%1
set database=%2

:: Sql command to add a new lock to the database
:: If the command fails, then run it again
sqlcmd -S %server% -d %database% -Q "INSERT INTO LockTable (LockID, UPI) VALUES(1, 'MACHINE\unknown\1234\070809')"

GOTO end

:error
echo Server and Database name must be specified
echo
echo Usage: lockDB (ServerName) (DatabaseName)

:end
echo
pause
