@ECHO OFF
CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

REM Get specified version of files from Common dir as well as AttributeFinder\build dir
IF NOT EXIST ..\..\Common mkdir ..\..\Common

IF "%BUILD_FROM_SVN%"=="YES" (
	CD ..\..\Common
	"C:\Program Files\CollabNet Subversion\svn.exe" export "%SVN_REPOSITORY%/tags/%~1/Engineering/ProductDevelopment/Common" .\ --force
	CD ..\AttributeFinder\Build
	"C:\Program Files\CollabNet Subversion\svn.exe" export "%SVN_REPOSITORY%/tags/%~1/Engineering/ProductDevelopment/IDShieldOffice/Build" .\ --force
) ELSE (
	CD ..\..\Common
	vault GETLABEL -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% -nonworkingfolder "%~p0\..\..\Common" "$/Engineering/ProductDevelopment/Common" %1
	CD ..\IDShieldOffice\Build
	vault GETLABEL -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% -nonworkingfolder "%~p0" "$/Engineering/ProductDevelopment/IDShieldOffice/Build" %1
)

Rem Remove previous build directory if it exists
IF EXIST %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT% RMDIR /S /Q %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%

IF NOT EXIST %BUILD_DRIVE%%BUILD_DIRECTORY% MKDIR %BUILD_DRIVE%%BUILD_DIRECTORY%
SET LOGFILE=%BUILD_DRIVE%%BUILD_DIRECTORY%\IDShieldOffice.log

nmake /F IDShieldOffice.mak BuildConfig="Release" ProductRootDirName="%PRODUCT_ROOT%" ProductVersion="%~1" DoEverything 2>&1 | tee "%LOGFILE%"

REM remove the drive mappings
net use P: /DELETE
net use R: /DELETE

:exit_script

