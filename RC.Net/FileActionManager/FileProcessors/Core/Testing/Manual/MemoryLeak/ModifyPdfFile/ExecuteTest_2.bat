REM Clean Source and Destination folders
call Clean.bat Test_1

REM Create Source direcrtory
md Source

REM Supply numbered file sets every 2 seconds for 6 hours
START "" CopyNumberedSets SourceImages Source 2 6

REM Execute command-line for desired test
START "" "C:\Program Files (x86)\Extract Systems\CommonComponents\ProcessFiles.exe" 2_ModifyPdfFile_Hyperlinks.fps /s

REM Start Logging Statistics every minute to a numbered subfolder
"C:\Program Files (x86)\Extract Systems\CommonComponents\LogProcessStats.exe" ProcessFiles 1m .\Stats\Test_2 /el
