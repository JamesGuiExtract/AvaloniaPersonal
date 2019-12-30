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

REM Run shell as admin (example) - put here code as you like
@ECHO Uninstalling all Extract Systems applications...
call "%~dp0UninstallExtract.bat"

:: Setup the paths for the installs
:: Assumes the internal build location is in SilentInstalls parallel to all the product installs
:: Assumes release build location has a Other\SilentInstalls folder with the product installs
::		in a folder parallel to Other and the product folder has the SetupFiles folder which is
::		the same as the internal build root folder
SET LABDE_ROOT=%~dp0
SET FLEXINDEX_ROOT=%~dp0
SET IDSHIELD_ROOT=%~dp0

:: Replaces the Other\SilentInstalls path if it exists with the Release path 
CALL SET LABDE_ROOT=%LABDE_ROOT:Other\SilentInstalls=LabDE\SetupFiles\LabDE%
CALL SET FLEXINDEX_ROOT=%FLEXINDEX_ROOT:Other\SilentInstalls=FLEXIndex\SetupFiles\FLEXIndex%
CALL SET IDSHIELD_ROOT=%IDSHIELD_ROOT:Other\SilentInstalls=IDShield\SetupFiles\IDShield%

:: If replace the SilentInstalls with the product folder 
CALL SET LABDE_ROOT=%LABDE_ROOT:SilentInstalls=LabDE%
CALL SET FLEXINDEX_ROOT=%FLEXINDEX_ROOT:SilentInstalls=FLEXIndex%
CALL SET IDSHIELD_ROOT=%IDSHIELD_ROOT:SilentInstalls=IDShield%

IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	set LABDE_ISS="%~dp0LabDE64.iss"
	set FLEXINDEX_ISS="%~dp0FlexIndex64.iss"
	set IDSHIELD_ISS="%~dp0Idshield64.iss"
	set EXTRACT_COMMON=C:\Program Files (x86^)\Extract Systems\CommonComponents
	set IDSHIELD_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{158160CD-7B55-462F-8477-7E18B2937D40}"
	set FLEXINDEX_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{A7DFE34D-A07E-4D57-A624-B758E42A69D4}"
	set LABDE_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{0E412937-E4FA-4737-A321-00AED69497C7}"
) ELSE (
	set LABDE_ISS="%~dp0LabDE.iss"
	set FLEXINDEX_ISS="%~dp0FlexIndex.iss"
	set IDSHIELD_ISS="%~dp0Idshield.iss"
	set EXTRACT_COMMON=C:\Program Files\Extract Systems\CommonComponents
	set IDSHIELD_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{158160CD-7B55-462F-8477-7E18B2937D40}"
	set FLEXINDEX_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{A7DFE34D-A07E-4D57-A624-B758E42A69D4}"
	set LABDE_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{0E412937-E4FA-4737-A321-00AED69497C7}"
)

@ECHO.
@ECHO Installing LabDE
start /wait "" "%LABDE_ROOT%Setup" /s /w /f1%LABDE_ISS% /f2nul

:: Check registry for the uninstall for LabDE as verification that it installed
IF EXIST "%TEMP%\LabDEInstalled.reg" DEL "%TEMP%\LabDEInstalled.reg"
@regedit /e "%TEMP%\LabDEInstalled.reg" %LABDE_KEY%
IF NOT EXIST "%TEMP%\LabDEInstalled.reg" (
	@ECHO There was a error installing LabDE
	GOTO ERROR
)
DEL "%TEMP%\LabDEInstalled.reg"

@ECHO.
@ECHO Installing IDShield
start /wait "" "%IDSHIELD_ROOT%Setup" /s /w /f1%IDSHIELD_ISS% /f2nul

:: Check registry for the uninstall for ID Shield as verification that it installed
IF EXIST "%TEMP%\IDShieldInstalled.reg" DEL "%TEMP%\IDShieldInstalled.reg"
@regedit /e "%TEMP%\IDShieldInstalled.reg" %IDSHIELD_KEY%
IF NOT EXIST "%TEMP%\IDShieldInstalled.reg" (
	@ECHO There was a error installing ID Shield
	GOTO ERROR
)
DEL "%TEMP%\IDShieldInstalled.reg"

@ECHO.
@ECHO Installing FlexIndex
start /wait "" "%FLEXINDEX_ROOT%Setup" /s /w /f1%FLEXINDEX_ISS% /f2nul

:: Check registry for the uninstall for FlexIndex as verification that it installed
IF EXIST "%TEMP%\FlexIndexInstalled.reg" DEL "%TEMP%\FlexIndexInstalled.reg"
@regedit /e "%TEMP%\FlexIndexInstalled.reg" %FLEXINDEX_KEY%
IF NOT EXIST "%TEMP%\FlexIndexInstalled.reg" (
	@ECHO There was a error installing FLEX Index
	GOTO ERROR
)
DEL "%TEMP%\FlexIndexInstalled.reg"

call "%EXTRACT_COMMON%\RegisterAll.bat" /s

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
GOTO END

:: Handle common error tasks
:ERROR

:: Check if the .net framework prerequisite was installed
SET %NET_RELEASE%=
FOR /f "tokens=3" %%a in ('REG QUERY "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release ^|findstr /ri "REG_DWORD"') do set NET_RELEASE=%%a
IF %NET_RELEASE% LSS 393295 (
    @ECHO The .NET Framework 4.6 Prerequisite was not installed. Install manually before running the install again.
)
SET %NET_RELEASE%=
:END
@ECHO.

:endOfBatch
@PAUSE
