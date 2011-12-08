@ECHO OFF

IF "%~1"=="" GOTO missing_version_number_error
IF "%~2"=="" GOTO normal_build
IF "%~2"=="/patch" GOTO patch_build
IF "%~2"=="/PATCH" GOTO patch_build
IF "%~2"=="/Patch" GOTO patch_build
IF "%~2"=="/noget" GOTO no_get_build
GOTO invalid_second_argument_error

:missing_version_number_error
ECHO.
ECHO ***** ERROR *****
ECHO Please provide version number of product to build as the first argument!
ECHO.
GOTO exit_script

:invalid_second_argument_error
ECHO.
ECHO ***** ERROR *****
ECHO The second argument "%~2" is not recognized!
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
GOTO no_get

:init_build
CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

REM Get specified version of files from Common dir as well as AttributeFinder\build dir
IF NOT EXIST ..\..\Common mkdir ..\..\Common

IF "%BUILD_FROM_SVN%"=="YES" (
	CD ..\..\Common
	"C:\Program Files\CollabNet Subversion\svn.exe" export "%SVN_REPOSITORY%/tags/%~1/Engineering/ProductDevelopment/Common" .\ --force
	CD ..\AttributeFinder\Build
	"C:\Program Files\CollabNet Subversion\svn.exe" export "%SVN_REPOSITORY%/tags/%~1/Engineering/ProductDevelopment/AttributeFinder/Build" .\ --force
) ELSE (
	CD ..\..\Common
	vault GETLABEL -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% -nonworkingfolder "%~p0\..\..\Common" "$/Engineering/ProductDevelopment/Common" %1
	CD ..\AttributeFinder\Build
	vault GETLABEL -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% -nonworkingfolder "%~p0\" "$/Engineering/ProductDevelopment/AttributeFinder/Build" %1
)

:no_get

REM Get the build version number from the argument string
set VERSION_NUMBER=%1
:: Strip the quotes
set VERSION_NUMBER=%VERSION_NUMBER:~1,-1%
:: Remove the FlexIndex Ver. string
set VERSION_NUMBER=%VERSION_NUMBER:FlexIndex Ver. =%

Rem Remove previous build directory if it exists
IF EXIST %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT% RMDIR /S /Q %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%

IF NOT EXIST %BUILD_DRIVE%%BUILD_DIRECTORY% MKDIR %BUILD_DRIVE%%BUILD_DIRECTORY%
SET LOGFILE=%BUILD_DRIVE%%BUILD_DIRECTORY%\%VERSION_NUMBER% AttributeFinderSDK.log
SET LOGFILE2=%BUILD_DRIVE%%BUILD_DIRECTORY%\%VERSION_NUMBER% RDT.log

REM Copy the license file to the release folder for EncryptFile will work
IF NOT EXIST %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%\Engineering\Binaries\Release MKDIR %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%\Engineering\Binaries\Release
copy %BUILD_DRIVE%\BuildMachine_RDT*.lic %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%\Engineering\Binaries\Release\

nmake /F AttributeFinderSDK.mak BuildConfig="Release" ProductRootDirName="%PRODUCT_ROOT%" ProductVersion="%~1" %BuildScriptTarget% 2>&1 | tee "%LOGFILE%"
IF "%BuildScriptTarget%"=="DoBuilds" GOTO exit_script
nmake /F RuleDevelopmentKit.mak BuildConfig="Release" ProductRootDirName="%PRODUCT_ROOT%" ProductVersion="%~1" DoEverything 2>&1 | tee "%LOGFILE2%"

:exit_script

REM remove the drive mappings
net use P: /DELETE
net use R: /DELETE

pause
