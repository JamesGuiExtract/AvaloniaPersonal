IF DEFINED programfiles(x86) SET programfiles=%programfiles(x86)%

REM Execute command-line for desired test
START "" "%programfiles%\Extract Systems\CommonComponents\ProcessFiles.exe" "Verify.fps" "/s"

REM Execute the auto-hotkey script
START "" ".\Verify.ahk"

REM Start Logging Statistics to numbered subfolder
"%programfiles%\Extract Systems\CommonComponents\LogProcessStats.exe" ProcessFiles.exe,SSOCR2.exe,XOCR32b.exe,AdjustImageResolution.exe,CleanupImage.exe,ESConvertToPDF.exe,ESConvertUSSToTxt.exe,ImageFormatConverter.exe,RedactFromXml.exe,RunRules.exe,SQLServerInfo.exe 5s .\Stats\Test_1 /el