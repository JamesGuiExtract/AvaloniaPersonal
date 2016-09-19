@ECHO OFF

SET Branch=

IF "%~1"=="" GOTO missing_version_number_error


:no_get_build
ECHO Initiating a build without get
SET BuildScriptTarget=DoEverythingNoGet
CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

REM Get the build version number from the argument string
set VERSION_NUMBER=%1
:: Strip the quotes
set VERSION_NUMBER=%VERSION_NUMBER:~1,-1%
:: Remove the FlexIndex Ver. string
set VERSION_NUMBER=%VERSION_NUMBER:FlexIndex Ver. =%

IF NOT EXIST %BUILD_DRIVE%%BUILD_DIRECTORY% MKDIR %BUILD_DRIVE%%BUILD_DIRECTORY%
SET LOGFILE=%BUILD_DRIVE%%BUILD_DIRECTORY%\%VERSION_NUMBER% AttributeFinderSDK.log
SET LOGFILE2=%BUILD_DRIVE%%BUILD_DIRECTORY%\%VERSION_NUMBER% RDT.log

IF %BUILD_STATUS% NEQ "Started" (
	SET BUILD_STATUS="Started"

	PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%~p0..\..\Common\PowerShell\SendBuildStatuseMail.ps1' '%~1' 'Started'"
)
IF EXIST "%TEMP%\nmakeErrors" del "%TEMP%\nmakeErrors"

nmake /X "%TEMP%\nmakeErrors" /F AttributeFinderSDK.mak BuildConfig="Release" ProductRootDirName="%PRODUCT_ROOT%" ProductVersion="%~1" %BuildScriptTarget% 2>&1 | tee "%LOGFILE%"
IF EXIST "%TEMP%\nmakeErrors" (
	FIND "NMAKE : fatal error" "%TEMP%\nmakeErrors"
:: If there were no errors nothing will be found and FIND will return an errorlevel of 1
	IF NOT ERRORLEVEL 1 (
		SET BUILD_STATUS="Failed"
		GOTO exit_script
	)
)

IF "%BuildScriptTarget%"=="DoBuilds" GOTO exit_script

IF EXIST "%TEMP%\nmakeErrors" del "%TEMP%\nmakeErrors"
nmake /X "%TEMP%\nmakeErrors" /F RuleDevelopmentKit.mak BuildConfig="Release" ProductRootDirName="%PRODUCT_ROOT%" ProductVersion="%~1" DoEverything 2>&1 | tee "%LOGFILE2%"
IF EXIST "%TEMP%\nmakeErrors" (
	FIND "NMAKE : fatal error" "%TEMP%\nmakeErrors"
:: If there were no errors nothing will be found and FIND will return an errorlevel of 1
	IF NOT ERRORLEVEL 1 (
		SET BUILD_STATUS="Failed"
		GOTO exit_script
	)
)
SET BUILD_STATUS="Completed successfully"

:exit_script

REM remove the drive mappings
net use P: /DELETE
net use R: /DELETE

subst /d z:

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%~p0..\..\Common\PowerShell\SendBuildStatuseMail.ps1' '%~1' '%BUILD_STATUS%'"

SET BUILD_STATUS=

pause
