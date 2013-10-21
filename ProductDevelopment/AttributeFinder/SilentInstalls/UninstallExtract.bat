@echo off

setlocal

SET InstallShieldFolder=%ProgramFiles(x86)%\InstallShield Installation Information
SET PROGRAM_ROOT=%ProgramFiles(x86)%

IF NOT "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	SET InstallShieldFolder=%ProgramFiles%\InstallShield Installation Information
	SET PROGRAM_ROOT=%ProgramFiles%
)

SET LabDE=%InstallShieldFolder%\{0E412937-E4FA-4737-A321-00AED69497C7}\setup.exe
SET IdShield=%InstallShieldFolder%\{158160CD-7B55-462F-8477-7E18B2937D40}\setup.exe
SET FlexIndex=%InstallShieldFolder%\{A7DFE34D-A07E-4D57-A624-B758E42A69D4}\setup.exe
SET LM=%InstallShieldFolder%\{EB8DE231-8B66-4DE6-A56D-39452D8CF35F}\setup.exe
SET IDShieldSPClient=%InstallShieldFolder%\{6BF486D7-B930-4B82-A0A0-D8450F5C7BAB}\setup.exe
SET RDT=%InstallShieldFolder%\{735E1622-3990-445F-9E5D-B0D7FDE292A3}\setup.exe

:UninstRDT
:: UnInstall RDT
IF NOT EXIST "%RDT%" GOTO UninstLabDE

start /wait "" "%RDT%"  -l0x0409  -uninst /s /w /f1"%~dp0rdtuninst.iss"

:UninstLabDE
:: UnInstall LabDE
IF NOT EXIST "%LabDE%" GOTO UninstFlexIndex

start /wait "" "%LabDE%" -l0x0409  -uninst /s /w /f1"%~dp0labdeuninst.iss"

:UninstFlexIndex

:: UnInstall FlexIndex
IF NOT EXIST "%FlexIndex%" GOTO UninstIDShield

start /wait "" "%FlexIndex%"  -l0x0409  -uninst /s /w /f1"%~dp0flexindexuninst.iss"

:UninstIDShield

:: UnInstall IDShield
IF NOT EXIST "%IdShield%" GOTO UninstLM

start /wait "" "%IdShield%" -l0x0409  -uninst /s /w /f1"%~dp0idshielduninst.iss"

:UninstLM

:: UnInstall LM
IF NOT EXIST "%LM%" GOTO UninstIDShieldSPClient

start /wait "" "%LM%"  -l0x0409  -uninst /s /w /f1"%~dp0lmuninst.iss"

:UninstIDShieldSPClient

:: UnInstall IDShieldSPClient
IF NOT EXIST "%IDShieldSPClient%" GOTO done

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

endlocal