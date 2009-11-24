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
Echo XP
if "%PROCESSOR_ARCHITECTURE%" == "x86" (
	"%~dp0WindowsXP-KB942288-v3-x86.exe" /passive /promptrestart
)

goto finished

:2003Server
Echo 2003 Server
if "%PROCESSOR_ARCHITECTURE%" == "x86" (
	"%~dp0WindowsServer2003-KB942288-v4-x86.exe" /passive /promptrestart
)
if "%PROCESSOR_ARCHITECTURE%" == "AMD64" (
	"%~dp0WindowsServer2003-KB942288-v4-x64.exe" /passive /promptrestart
)

goto finished

:Vista2008
if "%PROCESSOR_ARCHITECTURE%" == "x86" (
	wusa "%~dp0Windows6.0-KB942288-v2-x86.msu" /passive /promptrestart
)
if "%PROCESSOR_ARCHITECTURE%" == "AMD64" (
	wusa "%~dp0Windows6.0-KB942288-v2-x64.msu" /passive /promptrestart
)

goto finished

:finished
