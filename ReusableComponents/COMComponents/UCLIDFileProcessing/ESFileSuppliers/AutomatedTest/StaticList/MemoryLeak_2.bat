REM Clean Source folder
CALL Clean.bat Test_2

REM Supply 60000 numbered files
FOR %%i in (One Two Three Four Five Six) DO (
  START /B CopyNumberedFiles "dummy.pdf" ".\Source\%%i" 0ms -n10000
)

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_2.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles 5s .\Stats\Test_2 /el
