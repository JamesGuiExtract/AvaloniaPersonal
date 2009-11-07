REM Clean Source folder
call Clean.bat Test_1

REM Supply Numbered Files for 4 hours
REM START CopyNumberedFiles "..\..\..\AFCore\AutomatedTest\Images\Image1.tif.uss" ".\Source" 500ms -h4
START I:\Common\Engineering\Tools\Utils\CopyNumberedFiles "..\..\..\AFCore\AutomatedTest\Images\Image1.tif.uss" ".\Source" 500ms -h4

REM Execute command-line for desired test
REM START ProcessFiles.exe MemoryLeak_1.fps /s
START D:\Engineering\Binaries\Debug\ProcessFiles.exe MemoryLeak_1.fps /s

REM Start Logging Statistics to numbered subfolder
REM LogProcessStats ProcessFiles 5s .\Stats\Test_1 /el
I:\Common\Engineering\Tools\Utils\LogProcessStats ProcessFiles 5s .\Stats\Test_1 /el
