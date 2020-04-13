@ECHO OFF

:::::::::::::::::::::::::::::::::::::::::
:: Automatically check & get admin rights
:::::::::::::::::::::::::::::::::::::::::
@echo off
CLS 
ECHO.
ECHO =============================
ECHO Running Admin shell
ECHO =============================

:checkPrivileges 
NET FILE 1>NUL 2>NUL
if '%errorlevel%' == '0' ( goto gotPrivileges ) else ( goto getPrivileges ) 

:getPrivileges 

:: Determine if elevation is possible by checking registry to determine if uac is enabled
REG QUERY HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\System\ /v EnableLUA 2>&1 | FIND "0x1" >NUL
if ERRORLEVEL 1 (
	ECHO Must be an administrator to run this file.
	ECHO.
	GOTO endOfBatch
)

if '%1'=='ELEV' (shift & goto gotPrivileges)  
ECHO. 
ECHO **************************************
ECHO Invoking UAC for Privilege Escalation 
ECHO **************************************

setlocal DisableDelayedExpansion
set "batchPath=%~0"
setlocal EnableDelayedExpansion
ECHO Set UAC = CreateObject^("Shell.Application"^) > "%temp%\OEgetPrivileges.vbs" 
ECHO UAC.ShellExecute "!batchPath!", "ELEV", "", "runas", 1 >> "%temp%\OEgetPrivileges.vbs" 
"%temp%\OEgetPrivileges.vbs" 
exit /B 

:gotPrivileges 
::::::::::::::::::::::::::::
:START
::::::::::::::::::::::::::::
setlocal & pushd .

@ECHO Uninstalling all Extract Systems applications...
call "%~dp0UninstallExtract.bat"

:: Setup the paths for the installs
:: Assumes the internal build location is in SilentInstalls parallel to all the product installs
:: Assumes release build location has a Other\SilentInstalls folder with the product installs
::		in a folder parallel to Other and the product folder has the SetupFiles folder which is
::		the same as the internal build root folder
SET SOFTWARE_INSTALL_ROOT=%~dp0

:: If replace the SilentInstalls with the product folder 
CALL SET SOFTWARE_INSTALL_ROOT=%SOFTWARE_INSTALL_ROOT:SilentInstalls=Install%


IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	set SOFTWARE_INSTALL_ISS="%~dp0ExtractSoftware.iss"
	set EXTRACT_COMMON=C:\Program Files (x86^)\Extract Systems\CommonComponents
	set SOFTWARE_INSTALL_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{A7DFE34D-A07E-4D57-A624-B758E42A69D4}"
) ELSE (
	set SOFTWARE_INSTALL_ISS="%~dp0ExtractSoftware.iss"
	set EXTRACT_COMMON=C:\Program Files\Extract Systems\CommonComponents
	set SOFTWARE_INSTALL_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{A7DFE34D-A07E-4D57-A624-B758E42A69D4}"
)

@ECHO.
@ECHO Installing Extract Software...
start /wait "" "%SOFTWARE_INSTALL_ROOT%Setup" /s /f1%SOFTWARE_INSTALL_ISS% /f2nul

:: Check registry for the uninstall for FlexIndex as verification that it installed
IF EXIST "%TEMP%\ExtractSoftwareInstalled.reg" DEL "%TEMP%\ExtractSoftwareInstalled.reg"
@regedit /e "%TEMP%\ExtractSoftwareInstalled.reg" %SOFTWARE_INSTALL_KEY%
IF NOT EXIST "%TEMP%\ExtractSoftwareInstalled.reg" (
	@ECHO There was a error installing Extract Software
	GOTO ERROR
)
DEL "%TEMP%\ExtractSoftwareInstalled.reg"

call "%EXTRACT_COMMON%\RegisterAll.bat" /s

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
GOTO END

:: Handle common error tasks
:ERROR
:: Check if the .net framework prerequisite was installed
SET NET_RELEASE=
FOR /f "tokens=3" %%a in ('REG QUERY "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release ^|findstr /ri "REG_DWORD"') do set NET_RELEASE=%%a
IF %NET_RELEASE% LSS 528040 (
    @ECHO The .NET Framework 4.8 Prerequisite was not installed. Install manually before running the install again.
)
SET NET_RELEASE=

:END
@ECHO.
:endOfBatch
@PAUSE
