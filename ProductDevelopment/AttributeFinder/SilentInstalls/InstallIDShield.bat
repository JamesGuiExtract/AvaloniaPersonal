@ECHO OFF

@ECHO Uninstalling all Extract Systems applications...
start /wait "" "%~dp0..\ExtractUninstaller\ExtractUninstaller" /s /f2nul

IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	set ISSFILE="%~dp0IDShield64.iss"
) ELSE (
	set ISSFILE="%~dp0IDShield.iss"
)

@ECHO.
@ECHO Installing ID Shield
start /wait "" "%~dp0..\IDShield\Setup" /s /f1%ISSFILE% /f2nul

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
@ECHO.
@PAUSE
