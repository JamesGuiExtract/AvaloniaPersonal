REM Clean Source folder
call Clean.bat

REM Supply numbered file sets every 10 seconds for 6 hours
START CopyNumberedSets SourceImages TestArea 10 6

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak.fps /s

REM Start Logging Statistics to numbered subfolder
LogProcessStats ProcessFiles,SSOCR2,XOCR32b,AdjustImageResolution,CleanupImage,ESConvertToPDF,ESConvertUSSToTxt,ImageFormatConverter,RedactFromXml,RunRules,SQLServerInfo 5s .\Stats\Test_1 /el
