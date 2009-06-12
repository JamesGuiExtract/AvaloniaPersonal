@ECHO OFF

REM Set environment variables...

REM *************************************************************************************
REM ** Modify any of the following variables if your directories are setup differently
REM *************************************************************************************
SET LOCAL_VSS_ROOT=D:

SET VISUAL_STUDIO=C:\Program Files\Microsoft Visual Studio 8
SET VB_DIR=%VISUAL_STUDIO%\VB
SET VCPP_DIR=%VISUAL_STUDIO%\VC
SET DevEnvDir=%VISUAL_STUDIO%\Common7\IDE
SET VS_COMMON=C:\Program Files\Microsoft Visual Studio 8\Common7
SET VSS_DIR=C:\Program Files\Microsoft Visual Studio\Common\VSS

SET INSTALL_SHIELD_DIR=C:\Program Files\InstallShield\InstallShield 5.5 Professional Edition
SET WINZIP_DIR=C:\Program Files\WinZip

SET DEV_STUDIO9_DIR=C:\Program Files\InstallShield\DevStudio 9

SET SSDIR=I:\Common\Engineering\Vss2005
SET PATH=%windir%;%windir%\System32;I:\Common\Engineering\Tools\Utils;%VSS_DIR%\win32;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\Nuance_16\bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\LeadTools_16\Bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\RogueWave\bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\SafeNetUltraPro\Bin
SET PATH=%PATH%;%DevEnvDir%;%VCPP_DIR%\BIN;%VS_COMMON%\Tools;%VS_COMMON%\Tools\bin;%VCPP_DIR%\PlatformSDK\bin;C:\Program Files\Microsoft Visual Studio 8\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;%VCPP_DIR%\VCPackages


REM Building projects...

SET LOGFILE=%LOCAL_VSS_ROOT%\Engineering\BuildAllProject.log

REM *************************************************************************************
REM Projects built in Debug mode
REM *************************************************************************************

REM Build all projects in Debug mode...
nmake /I /F BuildAllProjects.mak BuildConfig="Debug" BuildAllProjects | tee "%LOGFILE%"

REM *************************************************************************************
REM ** By default, all projects will be built in Debug mode. You can comment out any
REM ** of the following builds to fit your purpose.
REM *************************************************************************************

REM Build InputFunnel in Debug mode...
REM nmake /I /F BuildAllProjects.mak BuildConfig="Debug" BuildInputFunnel | tee "%LOGFILE%"

REM Build IcoMap for ArcGIS in Debug mode...
REM nmake /I /F BuildAllProjects.mak BuildConfig="Debug" BuildIcoMapForArcGIS | tee "%LOGFILE%"

REM Build SwipeIt for ArcGIS in Debug mode...
REM nmake /I /F BuildAllProjects.mak BuildConfig="Debug" BuildSwipeItForArcGIS | tee "%LOGFILE%"

REM Build AttributeFinder in Debug mode...
REM nmake /I /F BuildAllProjects.mak BuildConfig="Debug" BuildAttributeFinder | tee "%LOGFILE%"


REM *************************************************************************************
REM Projects built in Release mode
REM *************************************************************************************

REM Build all projects in Release mode...
REM nmake /I /F BuildAllProjects.mak BuildConfig="Release" BuildAllProjects | tee "%LOGFILE%"

REM Build InputFunnel in Release mode...
REM nmake /I /F BuildAllProjects.mak BuildConfig="Release" BuildInputFunnel | tee "%LOGFILE%"

REM Build IcoMap for ArcGIS in Release mode...
REM nmake /I /F BuildAllProjects.mak BuildConfig="Release" BuildIcoMapForArcGIS | tee "%LOGFILE%"

REM Build SwipeIt for ArcGIS in Release mode...
REM nmake /I /F BuildAllProjects.mak BuildConfig="Release" BuildSwipeItForArcGIS | tee "%LOGFILE%"

REM Build AttributeFinder in Release mode...
REM nmake /I /F BuildAllProjects.mak BuildConfig="Release" BuildAttributeFinder | tee "%LOGFILE%"