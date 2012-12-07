@ECHO OFF
ECHO You completed redacting images at: >> %FooterFileName%
TIME /T >> %FooterFileName%
ECHO. >> %FooterFileName%
ECHO Thanks for playing the ID Shield Redaction Game! >> %FooterFileName%

REM Launch the Statistics Reporter
%CCPATH%\IDShieldStatisticsReporter.exe /Easy /FeedbackDataFolder C:\Demo_RedactionGame\Input /AutoRunSilent /CustomReport C:\Demo_RedactionGame\Misc\RedactionGameReport.txt /StatisticsOutputFolder C:\Demo_RedactionGame\Stats

REM Create the final report
COPY %HeaderFileName% + C:\Demo_RedactionGame\Stats\RedactionGameReport.txt + %FooterFileName% %FinalReportName%
notepad.exe /p %FinalReportName%
DEL %FinalReportName%
DEL %HeaderFileName%
DEL %FooterFileName%
