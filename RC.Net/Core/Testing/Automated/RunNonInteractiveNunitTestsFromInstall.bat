@ECHO OFF
SET ConsoleRunner='%~dp0nunit3-console.exe'
SET OutputFile=%~dp0TestResults-NonInteractive
SET Transform=%~dp0html-report.xslt
SET Script='%~dp0NUnitConsoleRunner.ps1'
SET DllDir='C:\Program Files (x86)\Extract Systems\CommonComponents'
SET Options=--where:'cat!=Interactive and cat!=Broken and cat!=OfficeRequired' --dispose-runners --result='%OutputFile%.xml' --result='%OutputFile%.htm;transform=%Transform%'

powershell -Command "& %Script% %ConsoleRunner% %DllDir% %Options%"

PAUSE
