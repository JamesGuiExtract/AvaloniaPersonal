:: Clean Source and Destination folders
call Clean.bat

:: Get the correct program files directory for 32/64-bit OS
if defined programfiles(x86) set programfiles=%programfiles(x86)%

:: Set component directory
set CommonComponentsDir=%programfiles%\Extract Systems\CommonComponents

:: Supply Numbered Files for 4 hours
START I:\Common\Engineering\Tools\Utils\CopyNumberedFiles\CopyNumberedFiles.exe ".\4PageTif.tif" ".\Source" 500ms -h4
pause

:: Execute command-line for desired test
START "%CommonComponentsDir%\ProcessFiles.exe" 1_ApplyBatesNewNumberEachDocumentTif.fps /spause

:: Start Logging Statistics every minute to a numbered subfolder
"%CommonComponentsDir%\LogProcessStats" ProcessFiles 1m .\Stats\Test_1 /el
