@ECHO OFF
SET ConsoleRunner='%~dp0..\..\..\..\Binaries\Debug\nunit3-console.exe'
SET OutputFile=%~dp0TestResults-NonInteractive
SET Transform=%~dp0html-report.xslt
SET Script='%~dp0NUnitConsoleRunner.ps1'
SET DllDir='%~dp0..\..\..\..\Binaries\Debug'
SET Options=--where:'cat!=Interactive and cat!=Broken' --dispose-runners --result='%OutputFile%.xml' --result='%OutputFile%.htm;transform=%Transform%'

powershell -Command "& %Script% %ConsoleRunner% %DllDir% %Options%"

PAUSE
