REM Clean Source folder
call Clean.bat Test_2

REM Supply Numbered Files for 4 hours
START I:\Common\Engineering\Tools\Utils\CopyNumberedFiles\CopyNumberedFiles.exe "..\..\..\AFCore\AutomatedTest\Images\Image1.tif.uss" ".\Source" 3s -h4

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_2.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats.exe ProcessFiles 5s .\Stats\Test_2 /el
