REM Clean Source folder
call Clean.bat

REM Supply numbered file sets every 5 minutes for 8 hours
START CopyNumberedSets SourceImages TestArea 300 8

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles.exe,SSOCR2.exe,XOCR32b.exe,AdjustImageResolution.exe,CleanupImage.exe,ESConvertToPDF.exe,ESConvertUSSToTxt.exe,ImageFormatConverter.exe,RedactFromXml.exe,RunRules.exe,SQLServerInfo.exe 5s .\Stats\Test_1 /el
