REM Clean Source and Destination folders
call Clean.bat

REM Ensure Sample.txt is readonly
attrib +R ".\Sample.txt"

REM Supply Numbered Files for 6 hours
START CopyNumberedFiles ".\Sample.txt" ".\Source" 1 -h6

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_16.fps /s

REM Start Logging Statistics every minute to a numbered subfolder
LogProcessStats ProcessFiles 1m .\Stats\Test_16 /el
