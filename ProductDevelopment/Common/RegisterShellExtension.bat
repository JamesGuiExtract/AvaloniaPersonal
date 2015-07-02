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
	ECHO This file must be ran as administrator
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

:: Register the dlls for 32 bit applications
%windir%\Microsoft.NET\Framework\v4.0.30319\regasm "%~dp0\LogicNP.EZShellExtensions.dll"
%windir%\Microsoft.NET\Framework\v4.0.30319\regasm "%~dp0\Extract.Utilities.ShellExtensions.dll"

:: If running on 64 bit OS both dlls for 64 bit
IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	%windir%\Microsoft.NET\Framework64\v4.0.30319\regasm "%~dp0\LogicNP.EZShellExtensions.dll"
	%windir%\Microsoft.NET\Framework64\v4.0.30319\regasm "%~dp0\Extract.Utilities.ShellExtensions.dll"
)

:endOfBatch