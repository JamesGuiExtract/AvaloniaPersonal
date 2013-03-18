@ECHO OFF

CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

REM Get the build version number from the argument string
set VERSION_NUMBER=%1
:: Strip the quotes
set VERSION_NUMBER=%VERSION_NUMBER:~1,-1%
:: Remove the FlexIndex Ver. string
set VERSION_NUMBER=%VERSION_NUMBER:FlexIDS for SP Ver. =%

REM Get specified version of files from Common dir as well as AttributeFinder\build dir
IF NOT EXIST %BUILD_DRIVE%\Engineering\Common mkdir %BUILD_DRIVE%\Engineering\Common

CD %BUILD_DRIVE%\Engineering\Common
vault GETLABEL -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% -nonworkingfolder "%BUILD_DRIVE%\Engineering\Common" "$/Engineering/ProductDevelopment/Common" %1
CD %BUILD_DRIVE%\Engineering\ProductDevelopment\AFIntegrations\SharePoint\Build
vault GETLABEL -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% -nonworkingfolder "%~p0\" "$/Engineering/ProductDevelopment/AFIntegrations/SharePoint/Build" %1

Rem Remove previous build directory if it exists
IF EXIST %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT% RMDIR /S /Q %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%

IF NOT EXIST %BUILD_DRIVE%%BUILD_DIRECTORY% MKDIR %BUILD_DRIVE%%BUILD_DIRECTORY%
SET LOGFILE=%BUILD_DRIVE%%BUILD_DIRECTORY%\%VERSION_NUMBER% FlexIDSSP.log

nmake /F FlexIDSSP.mak BuildConfig="Release" ProductRootDirName="%PRODUCT_ROOT%" ProductVersion="%~1" DoEverything 2>&1 | tee "%LOGFILE%"

:exit_script

REM remove the drive mappings
net use P: /DELETE
net use R: /DELETE

pause
