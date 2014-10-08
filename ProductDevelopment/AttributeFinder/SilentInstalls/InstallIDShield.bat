@ECHO OFF

@ECHO Uninstalling all Extract Systems applications...
call "%~dp0UninstallExtract.bat"

:: Setup the paths for the installs
:: Assumes the internal build location is in SilentInstalls parallel to all the product installs
:: Assumes release build location has a Other\SilentInstalls folder with the product installs
::		in a folder parallel to Other and the product folder has the SetupFiles folder which is
::		the same as the internal build root folder
SET IDSHIELD_ROOT=%~dp0

:: Replaces the Other\SilentInstalls path if it exists with the Release path 
CALL SET IDSHIELD_ROOT=%IDSHIELD_ROOT:Other\SilentInstalls=IDShield\SetupFiles%

:: If replace the SilentInstalls with the product folder 
CALL SET IDSHIELD_ROOT=%IDSHIELD_ROOT:SilentInstalls=IDShield%

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
start /wait "" "%IDSHIELD_ROOT%Setup" /s /f1%ISSFILE% /f2nul

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
