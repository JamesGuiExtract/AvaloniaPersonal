:: Clean Source and Destination folders
call Clean.bat Test_2

:: Get the correct program files directory for 32/64-bit OS
if defined programfiles(x86) set programfiles=%programfiles(x86)%

:: Set component directory
set CommonComponentsDir=%programfiles%\Extract Systems\CommonComponents

:: Supply Numbered Files for 6 hours
START "" "%CommonComponentsDir%\CopyNumberedFiles.exe" "..\..\..\..\..\..\..\..\ProductDevelopment\AttributeFinder\AFCore\AutomatedTest\Images\Image2.tif" ".\Source" 2s -h6

:: Execute command-line for desired test
START "" "%CommonComponentsDir%\ProcessFiles.exe" 2_RetrieveAttributes.fps /s

:: Start Logging Statistics every minute to a numbered subfolder
"%CommonComponentsDir%\LogProcessStats.exe" ProcessFiles 1m .\Stats\Test_2 /el
