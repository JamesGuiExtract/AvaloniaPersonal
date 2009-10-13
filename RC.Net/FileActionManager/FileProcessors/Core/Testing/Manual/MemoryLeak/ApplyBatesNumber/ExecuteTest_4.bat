:: Clean Source and Destination folders
call Clean.bat

:: Get the correct program files directory for 32/64-bit OS
if defined programfiles(x86) set programfiles=%programfiles(x86)%

:: Set component directory
set CommonComponentsDir=%programfiles%\Extract Systems\CommonComponents

:: Supply Numbered Files for 4 hours
START I:\Common\Engineering\Tools\Utils\CopyNumberedFiles\CopyNumberedFiles ".\4PagePdf.Pdf" ".\Source" 500ms -h4

:: Execute command-line for desired test
START "%CommonComponentsDir%\ProcessFiles.exe" 4_ApplyBatesNewNumberEachPagePdf.fps /s

:: Start Logging Statistics every minute to a numbered subfolder
"%CommonComponentsDir%\LogProcessStats.exe" ProcessFiles 1m .\Stats\Test_4 /el
