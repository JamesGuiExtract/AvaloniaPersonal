REM Clean Source folder
call Clean.bat

REM Supply Numbered Files for 6 hours
START CopyNumberedFiles ".\BorderSmoothDeleteDespeckle.tif" ".\Source" 1 -h6

REM Execute command-line for desired test
START ProcessFiles.exe 3_BorderSmoothDeleteDespeckle.fps /s

REM Start Logging Statistics every minute to a numbered subfolder
LogProcessStats ProcessFiles,SSOCR2,XOCR32b,AdjustImageResolution,CleanupImage,ESConvertToPDF,ESConvertUSSToTxt,ImageFormatConverter,RedactFromXml,RunRules,SQLServerInfo  1m .\Stats\Test_3 /el
