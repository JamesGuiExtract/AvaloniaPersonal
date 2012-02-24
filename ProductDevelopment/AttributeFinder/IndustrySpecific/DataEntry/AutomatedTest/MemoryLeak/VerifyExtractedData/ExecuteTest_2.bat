REM Copy image and VOA file every 2 seconds for 8 hours
START CopyNumberedSets.bat Original Input 2 8

REM Copy Rules folder from Demo_FLEXIndex to local Rules subfolder
XCOPY C:\Demo_FLEXIndex\Rules\*.* .\Rules\. /S /I /Y

REM DemoFlexIndex.sdf from Demo_FLEXIndex to local DatabaseFiles subfolder
XCOPY "C:\Demo_FLEXIndex\DataEntry Solution\Database Files\*.*" .\DataEntrySolution\DatabaseFiles\. /S /I /Y

REM Execute command-line for desired test
START "c:\Program Files\Extract Systems\CommonComponents\ProcessFiles.exe" "MemoryLeak.fps" "/s"

REM Execute the auto-hotkey script
START ".\SimpleChanges.ahk"

REM Start Logging Statistics to numbered subfolder
"c:\Program Files\Extract Systems\CommonComponents\LogProcessStats.exe" ProcessFiles.exe,SSOCR2.exe,XOCR32b.exe,AdjustImageResolution.exe,CleanupImage.exe,ESConvertToPDF.exe,ESConvertUSSToTxt.exe,ImageFormatConverter.exe,RedactFromXml.exe,RunRules.exe,SQLServerInfo.exe 5s .\Stats\Test_2 /el