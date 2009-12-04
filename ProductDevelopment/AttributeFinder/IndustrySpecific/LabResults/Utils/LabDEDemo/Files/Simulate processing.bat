@echo off
ECHO.
ECHO Copying VOA files...
ECHO.
copy /Y DemoFiles\Installs\CurrentDemo\VOA\*.* input\

REM Copy over USS files
ECHO.
ECHO Copying USS files...
ECHO.
copy DemoFiles\Installs\CurrentDemo\TIF\*.uss input\

ECHO Marking all files as ready for verification...
sqlcmd -Q "UPDATE [Demo_LabDE].[dbo].[FAMFile] SET [ASC_Verify] = 'P'"

ECHO.
ECHO Done.
pause
