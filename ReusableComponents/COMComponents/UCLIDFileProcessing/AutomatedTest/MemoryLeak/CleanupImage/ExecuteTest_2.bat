REM Clean Source folder
call Clean.bat

REM Supply Numbered Files for 6 hours
START CopyNumberedFiles ".\halftones.tif" ".\Source" 1 -h6

REM Execute command-line for desired test
START ProcessFiles.exe MemoryLeak_2.fps /s

REM Start Logging Statistics every minute to a numbered subfolder
LogProcessStats ProcessFiles,SSOCR2,XOCR32b,AdjustImageResolution,CleanupImage,ESConvertToPDF,ESConvertUSSToTxt,ImageFormatConverter,RedactFromXml,RunRules,SQLServerInfo  1m .\Stats\Test_2 /el
