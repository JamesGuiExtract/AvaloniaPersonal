@ECHO OFF

@ECHO Uninstalling all Extract Systems applications...
call "%~dp0UninstallExtract.bat"

IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	set ISSFILE="%~dp0IDShield64.iss"
	set EXTRACT_COMMON=C:\Program Files (x86^)\Extract Systems\CommonComponents
) ELSE (
	set ISSFILE="%~dp0IDShield.iss"
	set EXTRACT_COMMON=C:\Program Files\Extract Systems\CommonComponents
)

@ECHO.
@ECHO Installing ID Shield
start /wait "" "%~dp0..\IDShield\Setup" /s /f1%ISSFILE% /f2nul

call "%EXTRACT_COMMON%\RegisterAll.bat" /s

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
@ECHO.
@PAUSE
