REM Clean Source and Destination folders
call Clean.bat Test_3

REM Create Source direcrtory
md Source

REM Supply numbered files for 6 hours
START "" "C:\Program Files (x86)\Extract Systems\CommonComponents\CopyNumberedFiles.exe" "..\..\..\Automated\Resources\FlexIndex.xml" ".\Source" 400ms -h6

REM Execute command-line for desired test
START "" "C:\Program Files (x86)\Extract Systems\CommonComponents\ProcessFiles.exe" 3_ValidateXML_Syntax.fps /s

REM Start Logging Statistics every minute to a numbered subfolder
"C:\Program Files (x86)\Extract Systems\CommonComponents\LogProcessStats.exe" ProcessFiles 1m .\Stats\Test_3 /el
