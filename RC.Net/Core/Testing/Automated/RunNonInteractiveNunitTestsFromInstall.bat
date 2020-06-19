@ECHO OFF
SET ConsoleRunner='%~dp0nunit3-console.exe'
SET OutputFile='%~dp0TestResults-NonInteractive.xml'
SET Script='%~dp0NUnitConsoleRunner.ps1'
SET DllDir='C:\Program Files (x86)\Extract Systems\CommonComponents'
SET Options=--where:cat!=Interactive --dispose-runners --result=%OutputFile%

powershell -Command "& %Script% %ConsoleRunner% %DllDir% %Options%"

ECHO.
ECHO Tests have finished running. Test results output to: %OutputFile%
ECHO.
PAUSE
