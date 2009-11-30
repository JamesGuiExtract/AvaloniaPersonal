@Echo off
rem Batch file to determine the OS and whether it is 32 bit or 64 bit

rem Find the OS Version

rem  Test for XP should set error level to 0 if it is 1 otherwise
ver | find "Version 5.1"
rem  Test the errorlevel
if "%errorlevel%" == "0" goto xp

ver | find "Version 5.2"
if "%errorlevel%" == "0" goto 2003Server

rem  Test for Vista or 2008 Server
ver | find "Version 6"
if "%errorlevel%" == "0" goto Vista2008

goto finished

:xp
if "%PROCESSOR_ARCHITECTURE%" == "x86" (
	"%~dp0..\Powershell\WindowsXP-KB926139-v2-x86-ENU.exe" /passive
)

goto finished

:2003Server
if "%PROCESSOR_ARCHITECTURE%" == "x86" (
	"%~dp0..\Powershell\WindowsServer2003-KB926139-v2-x86-ENU.exe" /passive
)
if "%PROCESSOR_ARCHITECTURE%" == "AMD64" (
	"%~dp0..\Powershell\WindowsServer2003.WindowsXP-KB926139-v2-x64-ENU.exe" /passive
)

goto finished

:Vista2008
rem Since can't determine if this is 2008 or vista try installing for 2008 then if that is not successful attempt the other
servermanagercmd -install powershell

if "%errorlevel%" == "0" goto finished

if "%PROCESSOR_ARCHITECTURE%" == "x86" (
	wusa "%~dp0..\Powershell\Windows6.0-KB928439-x86.msu" /passive
)
if "%PROCESSOR_ARCHITECTURE%" == "AMD64" (
	wusa "%~dp0..\Powershell\Windows6.0-KB928439-x64.msu" /passive
)

goto finished

:finished
if "%PROCESSOR_ARCHITECTURE%" == "x86" (
	"%~dp0SQLManagementStudio_x86_ENU" /qs /ACTION=Install /FEATURES=SQL,Tools /INSTANCENAME=MSSQLSERVER /SQLSVCACCOUNT="NT AUTHORITY\Network Service" /SQLSYSADMINACCOUNTS="BUILTIN\Administrators" /AGTSVCACCOUNT="NT AUTHORITY\Network Service"
)
if "%PROCESSOR_ARCHITECTURE%" == "AMD64" (
	"%~dp0SQLManagementStudio_x64_ENU" /qs /ACTION=Install /FEATURES=SQL,Tools /INSTANCENAME=MSSQLSERVER /SQLSVCACCOUNT="NT AUTHORITY\Network Service" /SQLSYSADMINACCOUNTS="BUILTIN\Administrators" /AGTSVCACCOUNT="NT AUTHORITY\Network Service"
)
