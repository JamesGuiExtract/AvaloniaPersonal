@ECHO OFF

SET BUILD_VSS_ROOT=%BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%
SET SSDIR=I:\Common\Engineering\Vss2009
SET PATH=%windir%;%windir%\System32;I:\Common\Engineering\Tools\Utils;%VSS_DIR%\win32;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\Nuance_18\bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\LeadTools_16.5\Bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\RogueWave\bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\SafeNetUltraPro\Bin
SET PATH=%PATH%;%DevEnvDir%;%VCPP_DIR%\BIN;%VS_COMMON%\Tools;%VS_COMMON%\Tools\bin;%VCPP_DIR%\PlatformSDK\bin;C:\Program Files\Microsoft Visual Studio 8\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;%VCPP_DIR%\VCPackages