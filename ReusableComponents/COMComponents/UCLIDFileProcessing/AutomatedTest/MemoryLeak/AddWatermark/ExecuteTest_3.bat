REM Clean Source folder
call Clean.bat

REM Supply Numbered Files for 6 hours
START CopyNumberedFiles ".\MultiPage.tif" ".\Source" 1 -h6

REM Execute command-line for desired test
START ProcessFiles.exe 3_24BitStampsTif.fps /s

REM Start Logging Statistics every minute to a numbered subfolder
LogProcessStats ProcessFiles 1m .\Stats\Test_3 /el
