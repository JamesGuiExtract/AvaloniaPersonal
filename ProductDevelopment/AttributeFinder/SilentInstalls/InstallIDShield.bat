@ECHO OFF

@ECHO Uninstalling all Extract Systems applications...
start /wait "" ..\ExtractUninstaller\ExtractUninstaller /s

IF EXIST "C:\Program Files (x86)" (
	set ISSFILE="%~dp0IDShield64.iss"
) ELSE (
	set ISSFILE="%~dp0IDShield.iss"
)

SET LM_ISS="%~dp0LM.iss"

@ECHO.
@ECHO Installing ID Shield
start /wait "" ..\IDShield\Setup /s /f1%ISSFILE% /f2nul

@ECHO.
@ECHO Installing Extract Systems LM
start /wait "" "..\Extract Systems LM\Setup" /s /f1%LM_ISS% /f2nul

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
@ECHO.
@PAUSE
