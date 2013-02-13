@ECHO OFF

setlocal

%~d0
cd %~p0

@ECHO Uninstalling all Extract Systems applications...
start /wait "" ..\ExtractUninstaller\ExtractUninstaller /s /f2nul

IF EXIST "C:\Program Files (x86)" (
	set LABDE_ISS="%~dp0LabDE64.iss"
) ELSE (
	set LABDE_ISS="%~dp0LabDE.iss"
)


@ECHO.
@ECHO Installing LabDE...
start /wait "" ..\LabDE\Setup /s /f1%LABDE_ISS% /f2nul


@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
@ECHO.
@PAUSE
