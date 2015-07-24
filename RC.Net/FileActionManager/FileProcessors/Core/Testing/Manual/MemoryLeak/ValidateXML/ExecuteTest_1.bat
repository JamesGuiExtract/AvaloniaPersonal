REM Clean Source and Destination folders
call Clean.bat Test_1

REM Create Source direcrtory
md Source

REM Supply numbered files for 4 hours
START "" "C:\Program Files (x86)\Extract Systems\CommonComponents\CopyNumberedFiles.exe" ".\LabDE-Inline.xml" ".\Source" 400ms -h4

REM Execute command-line for desired test
START "" "C:\Program Files (x86)\Extract Systems\CommonComponents\ProcessFiles.exe" 1_ValidateXML_Inline.fps /s

REM Start Logging Statistics every minute to a numbered subfolder
"C:\Program Files (x86)\Extract Systems\CommonComponents\LogProcessStats.exe" ProcessFiles 1m .\Stats\Test_1 /el
