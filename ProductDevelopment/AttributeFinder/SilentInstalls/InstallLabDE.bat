@ECHO OFF

@ECHO Uninstalling all Extract Systems applications...
call "%~dp0UninstallExtract.bat"

IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	set LABDE_ISS="%~dp0LabDE64.iss"
) ELSE (
	set LABDE_ISS="%~dp0LabDE.iss"
)

@ECHO.
@ECHO Installing LabDE...
start /wait "" "%~dp0..\LabDE\Setup" /s /f1%LABDE_ISS% /f2nul


@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
@ECHO.
@PAUSE
