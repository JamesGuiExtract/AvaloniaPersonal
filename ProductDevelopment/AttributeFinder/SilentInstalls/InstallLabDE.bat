@ECHO OFF

@ECHO Uninstalling all Extract Systems applications...
call "%~dp0UninstallExtract.bat"

IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	set LABDE_ISS="%~dp0LabDE64.iss"
	set EXTRACT_COMMON=C:\Program Files (x86^)\Extract Systems\CommonComponents
) ELSE (
	set LABDE_ISS="%~dp0LabDE.iss"
	set EXTRACT_COMMON=C:\Program Files\Extract Systems\CommonComponents
)

@ECHO.
@ECHO Installing LabDE...
start /wait "" "%~dp0..\LabDE\Setup" /s /f1%LABDE_ISS% /f2nul

call "%EXTRACT_COMMON%\RegisterAll.bat" /s

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
@ECHO.
@PAUSE
