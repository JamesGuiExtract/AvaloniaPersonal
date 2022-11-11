@ECHO OFF

IF "%~1"=="" GOTO missing_version_number_error

GOTO no_get_build

:missing_version_number_error
ECHO.
ECHO ***** ERROR *****
ECHO Please provide version number of product to build as the first argument!
ECHO.
GOTO exit_script

:: All builds are not get since changing to git - the working folder will be assumed to be set to appropriate branch
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

SET BUILD_STATUS=Started

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%~p0..\..\Common\PowerShell\SendBuildStatuseMail.ps1' '%~1' %BUILD_STATUS%"

:: This will be changed if the build fails
SET BUILD_STATUS=Succeeded

::Copy the license files from the ProductDevelop path on P:
copy /Y P:\AttributeFinder\BuildVMLicense\*.* "C:\ProgramData\Extract Systems\LicenseFiles"

@CD %EXTRACT_WEB_APP_REPO%
@FOR /f "delims=-" %%V in ('%GITPATH% describe --match v[0-9]*.[0-9]*.[0-9]* main') DO SET WebAppArchivePath=%WEB_BUILD_BASE_DIR%\%%V\%%V.zip
@CD "%~p0"

IF EXIST "%TEMP%\nmakeErrors" del "%TEMP%\nmakeErrors"
@ECHO FKBBuildNeeded = %FKBBuildNeeded%
nmake /X "%TEMP%\nmakeErrors" /F AttributeFinderSDK.mak FKBBuildNeeded=%FKBBuildNeeded% BuildConfig="Release" ProductRootDirName="%PRODUCT_ROOT%" ProductVersion="%~1" WebAppArchivePath=%WebAppArchivePath% %BuildScriptTarget% 2>&1 | tee "%LOGFILE%"
IF EXIST "%TEMP%\nmakeErrors" (
	FIND "NMAKE : fatal error" "%TEMP%\nmakeErrors"
:: If there were no errors nothing will be found and FIND will return an errorlevel of 1
	IF NOT ERRORLEVEL 1 (
		SET BUILD_STATUS=Failed
	)
)

IF /I %BUILD_STATUS%==Succeeded (
    PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%~p0..\..\Common\PowerShell\CreateMainISOs.ps1' '%~1' 'Internal'"
)

:exit_script

REM remove the drive mappings
net use P: /DELETE
net use R: /DELETE

subst /d z:

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%~p0..\..\Common\PowerShell\SendBuildStatuseMail.ps1' '%~1' '%BUILD_STATUS%'"

SET BUILD_STATUS=

pause
