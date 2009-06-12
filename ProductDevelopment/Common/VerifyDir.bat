@echo OFF
@REM VerifyDir.bat takes 2 argument.  
@REM VerifyDir <SourceDir> <DestDir>
@REM All files in All directory's under the <SourceDir> are compared to equivalant file in the <DestDir>
@REM The directories must not end with \

@REM Check to see if Source exist
@if not exist %~fs1\nul goto NoSourceDir

@REM Check to see if Destination exist
@if not exist %~fs2\nul goto NoDestDir

@REM Setup answer for comp 
Echo n >NoCmd.txt
for %%f in ( "%~1\*.*") do comp "%%~ff" "%~2\%%~nxf" <NoCmd.txt
Del NoCmd.txt

@REM Call VerifyDir Recursively for each subdir
For /D %%d in ("%~1\*.*") do Call "%~f0" "%%~fd" "%~2\%%~nxd"
Echo done
goto end

:NoSourceDir
@Echo Source Directory must exist
@Echo.
goto syntax

:NoDestDir
Echo Destination Directory must exist
@Echo.
goto syntax

:Syntax

@ECHO	Syntax: VerifyDir (SourceDir) (DestDir)
@ECHO.
@ECHO 	VerifyDir compares all files in all directory's under the (SourceDir) with the equivalant file in the (DestDir)
@ECHO 	The directories must not end with \


:end