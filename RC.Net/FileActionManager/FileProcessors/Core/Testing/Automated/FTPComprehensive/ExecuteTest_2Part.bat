REM Make a new copy of the files in source images every minute
START "" CopyNumberedSetsRecursive.bat SourceImages Upload 61 48

REM Execute FAM which will upload the files via an FTP task
START "" ProcessFiles.exe  "MemoryLeak_UploadPart.fps" "/s"

REM Execute FAM which will download the files via the FTP supplier
START "" ProcessFiles.exe  "MemoryLeak_DownloadPart.fps" "/s"

REM Start Logging Statistics to numbered subfolder
LogProcessStats.exe ProcessFiles.exe 5s .\Stats\Test_1 /el