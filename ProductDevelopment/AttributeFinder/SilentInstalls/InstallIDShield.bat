@ECHO OFF

@ECHO Uninstalling all Extract Systems applications...
call "%~dp0UninstallExtract.bat"

IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	set ISSFILE="%~dp0IDShield64.iss"
	set EXTRACT_COMMON=C:\Program Files (x86^)\Extract Systems\CommonComponents
	set IDSHIELD_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{158160CD-7B55-462F-8477-7E18B2937D40}"
) ELSE (
	set ISSFILE="%~dp0IDShield.iss"
	set EXTRACT_COMMON=C:\Program Files\Extract Systems\CommonComponents
	set IDSHIELD_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{158160CD-7B55-462F-8477-7E18B2937D40}"
)

@ECHO.
@ECHO Installing ID Shield
start /wait "" "%~dp0..\IDShield\Setup" /s /f1%ISSFILE% /f2nul

:: Check registry for the uninstall for ID Shield as verification that it installed
IF EXIST "%TEMP%\IDShieldInstalled.reg" DEL "%TEMP%\IDShieldInstalled.reg"
@regedit /e "%TEMP%\IDShieldInstalled.reg" %IDSHIELD_KEY%
IF NOT EXIST "%TEMP%\IDShieldInstalled.reg" (
	@ECHO There was a error installing ID Shield
	GOTO END
)
DEL "%TEMP%\IDShieldInstalled.reg"

call "%EXTRACT_COMMON%\RegisterAll.bat" /s

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
:END
@ECHO.
@PAUSE
