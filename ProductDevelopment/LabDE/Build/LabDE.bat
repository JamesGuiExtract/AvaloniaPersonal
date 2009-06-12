@ECHO OFF
CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

REM Get specified version of files from Common dir as well as AttributeFinder\build dir
IF NOT EXIST ..\..\Common mkdir ..\..\Common
IF "%BUILD_FROM_SVN%"=="YES" (
	CD ..\..\Common
	"C:\Program Files\CollabNet Subversion\svn.exe" export "%SVN_REPOSITORY%/tags/%~1/Engineering/ProductDevelopment/Common" .\ --force
	CD ..\LabDE\Build
	"C:\Program Files\CollabNet Subversion\svn.exe" export "%SVN_REPOSITORY%/tags/%~1/Engineering/ProductDevelopment/LabDE/Build" .\ --force
) ELSE (
	CD ..\..\Common
	ss get $/Engineering/ProductDevelopment/Common -R -I- -W -VL%1
	CD ..\LabDE\Build
	ss get $/Engineering/ProductDevelopment/LabDE/Build -R -I- -W -VL%1
)

Rem Remove previous build directory if it exists
IF EXIST %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT% RMDIR /S /Q %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%

IF NOT EXIST %BUILD_DRIVE%%BUILD_DIRECTORY% MKDIR %BUILD_DRIVE%%BUILD_DIRECTORY%
SET LOGFILE=%BUILD_DRIVE%%BUILD_DIRECTORY%\LabDE.log

REM Copy the license file to the release folder for EncryptFile will work
IF NOT EXIST %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%\Engineering\Binaries\Release MKDIR %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%\Engineering\Binaries\Release
copy %BUILD_DRIVE%\BuildMachine_RDT*.lic %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%\Engineering\Binaries\Release\

nmake /F LabDE.mak BuildConfig="Release" ProductRootDirName="%PRODUCT_ROOT%" ProductVersion="%~1" DoEverything 2>&1 | tee "%LOGFILE%"
