@ECHO OFF
SET ConsoleRunner='%~dp0..\..\..\APIs\NUnit.ConsoleRunner.3.11.1\tools\nunit3-console.exe'
SET OutputFile=%~dp0TestResults-NonInteractive
SET Transform=%~dp0html-report.xslt
SET Script='%~dp0NUnitConsoleRunner.ps1'
SET DllDir='%~dp0..\..\..\..\Binaries\Debug'
SET Options=--where:cat!=Interactive --dispose-runners --result='%OutputFile%.xml' --result='%OutputFile%.htm;transform=%Transform%' --agents=1

powershell -Command "& %Script% %ConsoleRunner% %DllDir% %Options%"

PAUSE
