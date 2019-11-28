REM %1 = resx file directory
REM %2 = TargetDir/FileNameBase
REM %3 = FrameworkSDKDir

SETLOCAL
PUSHD "%1"
SET "resgenPath=%~3bin\NETFX 4.6 Tools"
IF NOT EXIST "%resgenPath%" (
SET "resgenPath=%~3bin\NETFX 4.6.1 Tools"
)
IF NOT EXIST "%resgenPath%" (
SET "resgenPath=%~3bin\NETFX 4.8 Tools"
)

FOR /f %%f IN ('dir /b *.resx') DO CALL :CREATE_RESOURCE_FILE %%f %2

ENDLOCAL
GOTO :EOF

:CREATE_RESOURCE_FILE
IF /I NOT "%~n1" == "Default" (CALL "%resgenPath%\Resgen" "%1" "%~2.%~n1.resources")
