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
	"%~dp0\WindowsXP-KB926139-v2-x86-ENU.exe" /quiet
)

goto finished

:2003Server
if "%PROCESSOR_ARCHITECTURE%" == "x86" (
	"%~dp0\WindowsServer2003-KB926139-v2-x86-ENU.exe" /quiet
)
if "%PROCESSOR_ARCHITECTURE%" == "AMD64" (
	"%~dp0\WindowsServer2003.WindowsXP-KB926139-v2-x64-ENU.exe" /quiet
)

goto finished

:Vista2008
rem Since can't determine if this is 2008 or vista try installing for 2008 then if that is not successful attempt the other
servermanagercmd - install powershell

if "%errorlevel%" == "0" goto finished

if "%PROCESSOR_ARCHITECTURE%" == "x86" (
	wusa "%~dp0\Windows6.0-KB928439-x86.msu" /quiet
)
if "%PROCESSOR_ARCHITECTURE%" == "AMD64" (
	wusa "%~dp0\Windows6.0-KB928439-x64.msu" /quiet
)

goto finished

:finished