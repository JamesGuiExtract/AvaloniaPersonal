REM Clean Source folder
call Clean.bat Test_2

REM Supply Numbered Files for 4 hours
START CopyNumberedFiles.exe ".\Images\TestImage001.ssn.voa" ".\Source" 100ms -h4
START CopyNumberedFiles.exe ".\Images\TestImage001.dob.voa" ".\Source" 100ms -h4
START CopyNumberedFiles.exe ".\Images\TestImage008.ssn.voa" ".\Source" 100ms -h4
START CopyNumberedFiles.exe ".\Images\TestImage008.dob.voa" ".\Source" 100ms -h4
START CopyNumberedFiles.exe ".\Images\TestImage001.tif" ".\Source" 100ms -h4
START CopyNumberedFiles.exe ".\Images\TestImage008.tif" ".\Source" 100ms -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_2.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 1m .\Stats\Test_2 /el
