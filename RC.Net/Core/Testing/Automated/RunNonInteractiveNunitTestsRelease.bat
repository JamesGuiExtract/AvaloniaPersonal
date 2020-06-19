@ECHO OFF
SET ConsoleRunner='%~dp0..\..\..\APIs\NUnit.ConsoleRunner.3.11.1\tools\nunit3-console.exe'
SET OutputFile='%~dp0TestResults-NonInteractive.xml'
SET Script='%~dp0NUnitConsoleRunner.ps1'
SET DllDir='%~dp0..\..\..\..\Binaries\Release'
SET Options=--where:cat!=Interactive --dispose-runners --result=%OutputFile%

powershell -Command "& %Script% %ConsoleRunner% %DllDir% %Options%"

ECHO.
ECHO Tests have finished running. Test results output to: %OutputFile%
ECHO.
PAUSE
