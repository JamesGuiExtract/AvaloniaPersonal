::Set up PATH
if defined programfiles(x86) set programfiles=%programfiles(x86)%

set path=path;%programfiles%\extract systems\commoncomponents

::Clean Source folder
call Clean.bat

::Supply Numbered Files for 4 hours
START CopyNumberedFiles "..\..\..\AFCore\AutomatedTest\Images\Image2.tif.uss" ".\Source" 400ms -h4

::Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_1.fps /s

::Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles.exe,SSOCR2.exe,XOCR32b.exe,AdjustImageResolution.exe,CleanupImage.exe,ESConvertToPDF.exe,ESConvertUSSToTxt.exe,ImageFormatConverter.exe,RedactFromXml.exe,RunRules.exe,SQLServerInfo.exe 5s .\Stats\Test_1 /el
