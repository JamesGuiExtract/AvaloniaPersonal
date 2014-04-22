@ECHO OFF

@ECHO Uninstalling all Extract Systems applications...
call "%~dp0UninstallExtract.bat"

IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	set LABDE_ISS="%~dp0LabDE64.iss"
	set EXTRACT_COMMON=C:\Program Files (x86^)\Extract Systems\CommonComponents
	set LABDE_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{0E412937-E4FA-4737-A321-00AED69497C7}"
) ELSE (
	set LABDE_ISS="%~dp0LabDE.iss"
	set EXTRACT_COMMON=C:\Program Files\Extract Systems\CommonComponents
	set LABDE_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{0E412937-E4FA-4737-A321-00AED69497C7}"
)

@ECHO.
@ECHO Installing LabDE...
start /wait "" "%~dp0..\LabDE\Setup" /s /f1%LABDE_ISS% /f2nul

:: Check registry for the uninstall for LabDE as verification that it installed
IF EXIST "%TEMP%\LabDEInstalled.reg" DEL "%TEMP%\LabDEInstalled.reg"
@regedit /e "%TEMP%\LabDEInstalled.reg" %LABDE_KEY%
IF NOT EXIST "%TEMP%\LabDEInstalled.reg" (
	@ECHO There was a error installing LabDE
	GOTO END
)
DEL "%TEMP%\LabDEInstalled.reg"

call "%EXTRACT_COMMON%\RegisterAll.bat" /s

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
:END
@ECHO.
@PAUSE

