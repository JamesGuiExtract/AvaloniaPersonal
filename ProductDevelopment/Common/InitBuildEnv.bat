@ECHO OFF

SET BUILD_VSS_ROOT=%BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%

SET NUANCE_API_ROOT=%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\Nuance_20
SET NUANCE_API_DIR=%NUANCE_API_ROOT%\bin
SET LEADTOOLS_API_ROOT=%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\Leadtools_20
SET LEADTOOLS_API_DIR=%LEADTOOLS_API_ROOT%\Bin
SET LEADTOOLS_API_DOTNET=%LEADTOOLS_API_ROOT%\DotNet

SET PATH=%windir%;%windir%\System32;%windir%\System32\WindowsPowerShell\v1.0;I:\Common\Engineering\Tools\Utils;%NUANCE_API_DIR%;%LEADTOOLS_API_DIR%;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\SafeNetUltraPro\Bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\Inlite_5_7\bin
SET PATH=%PATH%;%DevEnvDir%;%VCPP_DIR%\Tools\MSVC\14.11.25503\bin\HostX86\x86;%VS_COMMON%\Tools;%VS_COMMON%\Tools\bin;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;%DOTFUSCATOR%;%FX_COP%;
SET PATH=%PATH%;%WINDOWS_SDK%\BIN;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\Leadtools_20\Dotnet
SET INSTALL_PRODUCT_DEVELOPMENT_PATH=\\extract.local\Eng\General\productdevelopment
SET ENGSVR_INTERNAL_BASE=D:\Internal
SET BUILD_PRODUCT_RELEASE=\\extract.local\Eng\Builds
SET NAS_BUILD_BASE=/volume8/Eng-Builds

net use i: \\extract.local\All

net use m: \\extract.local\Eng\General

subst z: %BUILD_VSS_ROOT%

REM p: should be mapped to the base location that the files will be placed for Install shield to build the install
REM r: should be mapped to the base location that the install will be moved after it is completed
IF DEFINED BUILD_FINISHED_INSTALL (
	net use r: %BUILD_FINISHED_INSTALL%
) ELSE (
	net use r: %BUILD_PRODUCT_RELEASE%
)

IF DEFINED BUILD_INSTALL_FILES (
	net use p: %BUILD_INSTALL_FILES%
	net use t: %BUILD_INSTALL_FILES%\AttributeFinder\RDTInstallation\Files\TestFiles
) ELSE (
	net use p: %INSTALL_PRODUCT_DEVELOPMENT_PATH%
	net use t: %INSTALL_PRODUCT_DEVELOPMENT_PATH%\AttributeFinder\RDTInstallation\Files\TestFiles
)

IF DEFINED BUILD_INTERNAL_INSTALLS (
	net use s: %BUILD_INTERNAL_INSTALLS%
) ELSE (
	net use s: \\extract.local\Eng\General\ProductReleases_InternalUseOnly
)

