REM Execute command-line for desired test
START "" ProcessFiles.exe  "MemoryLeak.fps" "/s"

REM Start Logging Statistics to numbered subfolder
LogProcessStats.exe ProcessFiles.exe,SSOCR2.exe,XOCR32b.exe,AdjustImageResolution.exe,CleanupImage.exe,ESConvertToPDF.exe,ESConvertUSSToTxt.exe,ImageFormatConverter.exe,RedactFromXml.exe,RunRules.exe,SQLServerInfo.exe 5s .\Stats\Test_1 /el