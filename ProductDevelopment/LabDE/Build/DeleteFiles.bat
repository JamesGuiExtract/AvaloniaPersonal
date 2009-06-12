@ECHO OFF

DEL /F /S /Q %1

IF %errorlevel% == 1 goto errhandle

:errhandle
