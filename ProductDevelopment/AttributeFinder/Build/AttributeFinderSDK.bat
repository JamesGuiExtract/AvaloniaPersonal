@ECHO OFF

SET Branch=

IF "%~1"=="" GOTO missing_version_number_error
IF "%~2"=="" GOTO normal_build
IF "%~2"=="/noget" GOTO no_get_build

:: Assume second argument is the branch to build
ECHO.
ECHO Setting Branch to %~2
SET Branch=%2
GOTO normal_build

:missing_version_number_error
ECHO.
ECHO ***** ERROR *****
ECHO Please provide version number of product to build as the first argument!
ECHO.
GOTO exit_script

:patch_build
ECHO Initiating a patch build....
SET BuildScriptTarget=DoBuilds
GOTO init_build

:normal_build
ECHO Initiating a normal (non-patch) build....
SET BuildScriptTarget=DoEverything
GOTO init_build

:no_get_build
ECHO Initiating a build without get
SET BuildScriptTarget=DoEverythingNoGet
CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat
REM vcvars32 initializes command-line build vars so that editbin can be called to re-apply properties post-obfuscation
REM Not needed because the correct path already gets included in the Path
REM CALL "%VCPP_DIR%\Bin\vcvars32.bat"
GOTO no_get

:init_build
CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat
REM vcvars32 initializes command-line build vars so that editbin can be called to re-apply properties post-obfuscation
REM Not needed because the correct path already gets included in the Path
REM CALL "%VCPP_DIR%\Bin\vcvars32.bat"

REM Don't need to get the common and AttributeFinder build folders because either they will have been gotten manually or thru
REM the LabeFromLatestAndBuild.bat file that calls this
REM Get specified version of files from Common dir as well as AttributeFinder\build dir
REM IF NOT EXIST ..\..\Common mkdir ..\..\Common

REM IF "%BUILD_FROM_SVN%"=="YES" (
	REM CD ..\..\Common
	REM "C:\Program Files\CollabNet Subversion\svn.exe" export "%SVN_REPOSITORY%/tags/%~1/Engineering/ProductDevelopment/Common" .\ --force
	REM CD ..\AttributeFinder\Build
	REM "C:\Program Files\CollabNet Subversion\svn.exe" export "%SVN_REPOSITORY%/tags/%~1/Engineering/ProductDevelopment/AttributeFinder/Build" .\ --force
REM ) ELSE (
	REM CD ..\..\Common
	REM vault GETLABEL -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% -nonworkingfolder "%~p0\..\..\Common" "$%Branch%/Engineering/ProductDevelopment/Common" %1
	REM CD ..\AttributeFinder\Build
	REM vault GETLABEL -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% -nonworkingfolder "%~p0\" "$%Branch%/Engineering/ProductDevelopment/AttributeFinder/Build" %1
REM )

Rem Remove previous build directory if it exists
IF EXIST %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT% RMDIR /S /Q %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%

:no_get

REM Get the build version number from the argument string
set VERSION_NUMBER=%1
:: Strip the quotes
set VERSION_NUMBER=%VERSION_NUMBER:~1,-1%
:: Remove the FlexIndex Ver. string
set VERSION_NUMBER=%VERSION_NUMBER:FlexIndex Ver. =%

IF NOT EXIST %BUILD_DRIVE%%BUILD_DIRECTORY% MKDIR %BUILD_DRIVE%%BUILD_DIRECTORY%
SET LOGFILE=%BUILD_DRIVE%%BUILD_DIRECTORY%\%VERSION_NUMBER% AttributeFinderSDK.log
SET LOGFILE2=%BUILD_DRIVE%%BUILD_DIRECTORY%\%VERSION_NUMBER% RDT.log

REM Copy the license file to the release folder for EncryptFile will work
IF NOT EXIST %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%\Engineering\Binaries\Release MKDIR %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%\Engineering\Binaries\Release
copy %BUILD_DRIVE%\BuildMachine_RDT*.lic %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%\Engineering\Binaries\Release\

nmake /F AttributeFinderSDK.mak Branch=%Branch% BuildConfig="Release" ProductRootDirName="%PRODUCT_ROOT%" ProductVersion="%~1" %BuildScriptTarget% 2>&1 | tee "%LOGFILE%"

IF "%BuildScriptTarget%"=="DoBuilds" GOTO exit_script
nmake /F RuleDevelopmentKit.mak Branch=%Branch% BuildConfig="Release" ProductRootDirName="%PRODUCT_ROOT%" ProductVersion="%~1" DoEverything 2>&1 | tee "%LOGFILE2%"

:exit_script

REM remove the drive mappings
net use P: /DELETE
net use R: /DELETE

pause
