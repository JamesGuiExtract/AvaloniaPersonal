SET Server=HPQC2
SET email="christopher_wendt@extractsystems.com"
if defined programfiles(x86) (
::set programfiles=%programfiles(x86)%
goto sixtyfour
)
::SET ARG=C:\Program Files\Extract Systems\CommonComponents\FileProcessingComponents\Reports\Standard reports\Summary of actions and associated document page counts.rpt


:thirtytwo
cd "C:\Program Files\Extract Systems\CommonComponents"
"ReportViewer.exe" %Server% Endurance_Test "C:\Program Files\Extract Systems\CommonComponents\FileProcessingComponents\Reports\Standard reports\Summary of actions and associated document page counts.rpt" /mailto %email% /subject "Endurance Test"
goto end

:sixtyfour
cd "C:\Program Files (x86)\Extract Systems\CommonComponents"
"ReportViewer.exe" %Server% Endurance_Test "C:\Program Files (x86)\Extract Systems\CommonComponents\FileProcessingComponents\Reports\Standard reports\Summary of actions and associated document page counts.rpt" /mailto %email% /subject "Endurance Test"
goto end


:end
call WAIT.bat 900
taskkill /IM ReportViewer.exe /F