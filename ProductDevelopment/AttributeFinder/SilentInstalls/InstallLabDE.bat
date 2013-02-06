@ECHO OFF

@ECHO Uninstalling all Extract Systems applications...
start /wait "" ..\ExtractUninstaller\ExtractUninstaller /s

IF EXIST "C:\Program Files (x86)" (
	set LABDE_ISS="%~dp0LabDE64.iss"
) ELSE (
	set LABDE_ISS="%~dp0LabDE.iss"
)

SET LM_ISS="%~dp0LM.iss"

@ECHO.
@ECHO Installing LabDE...
start /wait "" ..\LabDE\Setup /s /f1%LABDE_ISS% /f2nul

@ECHO.
@ECHO Installing Extract Systems LM
start /wait "" "..\Extract Systems LM\Setup" /s /f1%LM_ISS% /f2nul

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
@ECHO.
@PAUSE
