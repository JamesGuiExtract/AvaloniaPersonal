@ECHO OFF

SET BUILD_VSS_ROOT=%BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%
SET VAULT_SERVER=white.extract.local
SET VAULT_REPOSITORY="Extract"
SET PATH=%windir%;%windir%\System32;I:\Common\Engineering\Tools\Utils;%VAULT_DIR%;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\Nuance_16.3\bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\LeadTools_16.5\Bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\RogueWave\bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\SafeNetUltraPro\Bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\Inlite_5_7\bin
SET PATH=%PATH%;%DevEnvDir%;%VCPP_DIR%\BIN;%VS_COMMON%\Tools;%VS_COMMON%\Tools\bin;%VCPP_DIR%\PlatformSDK\bin;C:\Program Files\Microsoft Visual Studio 8\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;%VCPP_DIR%\VCPackages;C:\Program Files\PreEmptive Solutions\Dotfuscator Professional Edition 4.6;C:\Program Files\Microsoft FxCop 1.36;
SET INSTALL_PRODUCT_DEVELOPMENT_PATH=\\fnp2\internal\Common\Engineering\productdevelopment

REM p: should be mapped to the base location that the files will be placed for Install shield to build the install
REM r: should be mapped to the base location that the install will be moved after it is completed
IF DEFINED BUILD_FINISHED_INSTALL (
	net use r: %BUILD_FINISHED_INSTALL%
) ELSE (
	net use r: \\fnp2\internal\Common\Engineering\ProductReleases
)

IF DEFINED BUILD_INSTALL_FILES (
	net use p: %BUILD_INSTALL_FILES%
) ELSE (
	net use p: %INSTALL_PRODUCT_DEVELOPMENT_PATH%
)

IF DEFINED BUILD_INTERNAL_INSTALLS (
	net use s: %BUILD_INTERNAL_INSTALLS%
) ELSE (
	net use s: \\fnp2\internal\Common\Engineering\ProductReleases_InternalUseOnly
)
