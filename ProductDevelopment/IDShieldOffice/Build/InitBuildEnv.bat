@ECHO OFF

SET BUILD_VSS_ROOT=%BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%
SET SSDIR=I:\Common\Engineering\Vss2009
SET PATH=%windir%;%windir%\System32;I:\Common\Engineering\Tools\Utils;%VSS_DIR%\win32;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\Nuance_16\bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\LeadTools_16\Bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\RogueWave\bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\SafeNetUltraPro\Bin
SET PATH=%PATH%;%DevEnvDir%;%VCPP_DIR%\BIN;%VS_COMMON%\Tools;%VS_COMMON%\Tools\bin;%VCPP_DIR%\PlatformSDK\bin;C:\Program Files\Microsoft Visual Studio 8\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;%VCPP_DIR%\VCPackages;C:\Program Files\Microsoft FxCop 1.36;C:\Program Files\PreEmptive Solutions\Dotfuscator Professional Edition 4.5;C:\Program Files\CollabNet Subversion

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
	net use p: \\fnp2\internal\Common\Engineering\productdevelopment
)