@echo off

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



SET InstallShieldFolder=%ProgramFiles(x86)%\InstallShield Installation Information
SET PROGRAM_ROOT=%ProgramFiles(x86)%

IF NOT "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	SET InstallShieldFolder=%ProgramFiles%\InstallShield Installation Information
	SET PROGRAM_ROOT=%ProgramFiles%
)

SET LabDE_GUID={0E412937-E4FA-4737-A321-00AED69497C7}
SET IdShield_GUID={158160CD-7B55-462F-8477-7E18B2937D40}
SET FlexIndex_GUID={A7DFE34D-A07E-4D57-A624-B758E42A69D4} // this is also the same as the ExtractInstaller
SET LM_GUID={EB8DE231-8B66-4DE6-A56D-39452D8CF35F}
SET IDShieldSPClient_GUID={6BF486D7-B930-4B82-A0A0-D8450F5C7BAB}
SET RDT_GUID={735E1622-3990-445F-9E5D-B0D7FDE292A3}

SET LabDE=%InstallShieldFolder%\%LabDE_GUID%\setup
SET IdShield=%InstallShieldFolder%\%IdShield_GUID%\setup
SET FlexIndex=%InstallShieldFolder%\%FlexIndex_GUID%\setup
SET LM=%InstallShieldFolder%\%LM_GUID%\setup
SET IDShieldSPClient=%InstallShieldFolder%\%IDShieldSPClient_GUID%\setup
SET RDT=%InstallShieldFolder%\%RDT_GUID%\setup

:: Make sure Clear image INSTALL.LOG file exists before running any of the
:: uninstalls since this will cause a dialog to pop up.
SET CLEARIMAGE_DIR=%PROGRAM_ROOT%\Inlite\ClearImage 7 PDK

:: Check if UNWISE.exe exists
IF EXIST "%CLEARIMAGE_DIR%\UNWISE.EXE" (
:: if the INSTALL.LOG file does not exist delete the unwise.exe file
	IF  NOT EXIST "%CLEARIMAGE_DIR%\INSTALL.LOG" (
		del "%CLEARIMAGE_DIR%\UNWISE.EXE"
	)
)

:UninstRDT
:: UnInstall RDT
IF NOT EXIST "%RDT%.ini" GOTO UninstLabDE

start /wait "" "%RDT%"  -l0x0409  /removeonly /s /w /f1"%~dp0rdtuninst.iss"

:UninstLabDE
:: UnInstall LabDE
IF NOT EXIST "%LabDE%.ini" GOTO UninstFlexIndex

start /wait "" "%LabDE%" -l0x0409  /removeonly /s /w /f1"%~dp0labdeuninst.iss"

:UninstFlexIndex

:: UnInstall FlexIndex or ExtractInstaller (works for both)
IF NOT EXIST "%FlexIndex%.ini" GOTO UninstIDShield

start /wait "" "%FlexIndex%"  -l0x0409  /removeonly /s /w /f1"%~dp0flexindexuninst.iss"

:UninstIDShield

:: UnInstall IDShield
IF NOT EXIST "%IdShield%.ini" GOTO UninstLM

start /wait "" "%IdShield%" -l0x0409  /removeonly /s /w /f1"%~dp0idshielduninst.iss"

:UninstLM

:: UnInstall LM
IF NOT EXIST "%LM%.ini" GOTO UninstIDShieldSPClient

start /wait "" "%LM%"  -l0x0409  -uninst /s /w /f1"%~dp0lmuninst.iss"

:UninstIDShieldSPClient

:: UnInstall IDShieldSPClient
IF NOT EXIST "%IDShieldSPClient%.ini" GOTO done

start /wait "" "%IDShieldSPClient%"  -l0x0409  -uninst /s /w 

:done

:: UnInstall Extract IDShield for Sharepoint Client
MsiExec.exe /x{6BF486D7-B930-4B82-A0A0-D8450F5C7BAB} /qn

:: UnInstall SQL Compact 3.5 SP2
msiexec /x{3A9FC03D-C685-4831-94CF-4EDFD3749497} /qn
MsiExec.exe /x{D4AD39AD-091E-4D33-BB2B-59F6FCB8ADC3} /qn

:: UnInstall SQL Compact 3.5 SP1
MsiExec.exe /x{F83779DF-E1F5-43A2-A7BE-732F856FADB7} /qn
MsiExec.exe /x{E59113EB-0285-4BFD-A37A-B79EAC6B8F4B} /qn

:: UnInstall Crystal Reports
MsiExec.exe /x{C484CC8D-03CF-4022-89C4-DB4F02E8A15B} /qn

:: UnInstall SQL Server 2008 R2 Native Client
MsiExec.exe /x{4AB6A079-178B-4144-B21F-4D1AE71666A2} /qn
MsiExec.exe /x{2180B33F-3225-423E-BBC1-7798CFD3CD1F} /qn

:: UnInstall SQL Server 2008 Native Client
MsiExec.exe /x{C79A7EAB-9D6F-4072-8A6D-F8F54957CD93} /qn

:: UnInstall ClearImage
SET CLEARIMAGE_DIR=%PROGRAM_ROOT%\Inlite\ClearImage 7 PDK

IF EXIST "%CLEARIMAGE_DIR%\UNWISE.EXE" (
	for %%f in ("%CLEARIMAGE_DIR%") do %%~sf\UNWISE.EXE /s %%~sf\INSTALL.LOG
)

:: Remove keys in the registry that should have been removed
IF NOT "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	IF NOT EXIST "%LabDE%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\InstallShield_%LabDE_GUID% /f 2>NUL
	IF NOT EXIST "%IdShield%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\InstallShield_%IdShield_GUID% /f 2>NUL
	IF NOT EXIST "%FlexIndex%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\InstallShield_%FlexIndex_GUID% /f 2>NUL
	IF NOT EXIST "%LM%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\InstallShield_%LM_GUID% /f 2>NUL
	IF NOT EXIST "%IDShieldSPClient%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\InstallShield_%IDShieldSPClient_GUID% /f 2>NUL
	IF NOT EXIST "%RDT%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\InstallShield_%RDT_GUID% /f 2>NUL	
    
	IF NOT EXIST "%LabDE%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\%LabDE_GUID% /f 2>NUL
	IF NOT EXIST "%IdShield%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\%IdShield_GUID% /f 2>NUL
	IF NOT EXIST "%FlexIndex%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\%FlexIndex_GUID% /f 2>NUL
	IF NOT EXIST "%LM%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\%LM_GUID% /f 2>NUL
	IF NOT EXIST "%IDShieldSPClient%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\%IDShieldSPClient_GUID% /f 2>NUL
	IF NOT EXIST "%RDT%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\%RDT_GUID% /f 2>NUL	
) ELSE (
	IF NOT EXIST "%LabDE%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\InstallShield_%LabDE_GUID% /f 2>NUL
	IF NOT EXIST "%IdShield%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\InstallShield_%IdShield_GUID% /f 2>NUL
	IF NOT EXIST "%FlexIndex%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\InstallShield_%FlexIndex_GUID% /f 2>NUL
	IF NOT EXIST "%LM%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\InstallShield_%LM_GUID% /f 2>NUL
	IF NOT EXIST "%IDShieldSPClient%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\InstallShield_%IDShieldSPClient_GUID% /f 2>NUL
	IF NOT EXIST "%RDT%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\InstallShield_%RDT_GUID% /f 2>NUL	
    
	IF NOT EXIST "%LabDE%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\%LabDE_GUID% /f 2>NUL
	IF NOT EXIST "%IdShield%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\%IdShield_GUID% /f 2>NUL
	IF NOT EXIST "%FlexIndex%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\%FlexIndex_GUID% /f 2>NUL
	IF NOT EXIST "%LM%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\%LM_GUID% /f 2>NUL
	IF NOT EXIST "%IDShieldSPClient%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\%IDShieldSPClient_GUID% /f 2>NUL
	IF NOT EXIST "%RDT%.ini" REG DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\%RDT_GUID% /f 2>NUL	
)

endlocal

:endOfBatch