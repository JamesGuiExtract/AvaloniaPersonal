@ECHO OFF

setlocal

%~d0
cd %~p0

@ECHO Uninstalling all Extract Systems applications...
start /wait "" ..\ExtractUninstaller\ExtractUninstaller /s

IF EXIST "C:\Program Files (x86)" (
	set ISSFILE="%~dp0IDShield64.iss"
) ELSE (
	set ISSFILE="%~dp0IDShield.iss"
)


@ECHO.
@ECHO Installing ID Shield
start /wait "" ..\IDShield\Setup /s /f1%ISSFILE% /f2nul


@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
@ECHO.
@PAUSE
