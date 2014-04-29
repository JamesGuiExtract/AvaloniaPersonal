@ECHO OFF

SET BUILD_VSS_ROOT=%BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%
SET VAULT_SERVER=EngSvr.extract.local
SET VAULT_REPOSITORY="Extract"

SET NUANCE_API_ROOT=%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\Nuance_19
SET NUANCE_API_DIR=%NUANCE_API_ROOT%\bin
SET LEADTOOLS_API_DIR=%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\LeadTools_17\Bin

SET PATH=%windir%;%windir%\System32;I:\Common\Engineering\Tools\Utils;%VAULT_DIR%;%NUANCE_API_DIR%;%LEADTOOLS_API_DIR%;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\SafeNetUltraPro\Bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\Inlite_5_7\bin
SET PATH=%PATH%;%DevEnvDir%;%VCPP_DIR%\BIN;%VS_COMMON%\Tools;%VS_COMMON%\Tools\bin;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;%VCPP_DIR%\VCPackages;%VCPP_DIR%\Bin;%DOTFUSCATOR%;%FX_COP%;
SET PATH=%PATH%;%WINDOWS_SDK%\BIN
SET INSTALL_PRODUCT_DEVELOPMENT_PATH=\\fnp2\internal\Common\Engineering\productdevelopment

net use i: \\fnp2\internal

net use m: \\engsvr\internal

REM p: should be mapped to the base location that the files will be placed for Install shield to build the install
REM r: should be mapped to the base location that the install will be moved after it is completed
IF DEFINED BUILD_FINISHED_INSTALL (
	net use r: %BUILD_FINISHED_INSTALL%
) ELSE (
	net use r: \\EngSvr\internal\ProductReleases
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
	net use s: \\EngSvr\internal\ProductReleases_InternalUseOnly
)

