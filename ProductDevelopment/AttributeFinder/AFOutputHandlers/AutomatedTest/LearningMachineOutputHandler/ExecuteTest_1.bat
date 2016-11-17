IF defined programfiles(x86) SET programfiles=%programfiles(x86)%
set ccdir=%programfiles%\extract systems\commoncomponents

REM Clean Source folder
call Clean.bat Test_1

REM Supply numbered file sets every 6 seconds for 10 hours
START CopyNumberedSets Images\AttributeCategorization Source\AttributeCategorization 6 10
START CopyNumberedSets Images\DocumentCategorization Source\DocumentCategorization 6 10
START CopyNumberedSets Images\Pagination Source\Pagination 6 10

REM Execute command-line for desired test
START "%ccdir%\ProcessFiles.exe" MemoryLeak_1.fps /s

REM Start Logging Statistics to numbered subfolder
"%ccdir%\LogProcessStats.exe" ProcessFiles.exe,SSOCR2.exe,XOCR32b.exe,AdjustImageResolution.exe,CleanupImage.exe,ESConvertToPDF.exe,ESConvertUSSToTxt.exe,ImageFormatConverter.exe,RedactFromXml.exe,RunRules.exe,SQLServerInfo.exe 5s .\Stats\Test_1 /el
