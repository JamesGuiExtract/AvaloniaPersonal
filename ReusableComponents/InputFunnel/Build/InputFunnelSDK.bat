@ECHO OFF
CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

REM Get specified version of files from Common dir as well as inputfunnel\build dir
CD ..\..\Common
ss get $/Engineering/ProductDevelopment/Common -R -I- -W -V%1
CD ..\InputFunnel\Build
ss get $/Engineering/ProductDevelopment/InputFunnel/Build -R -I- -W -V%1

IF NOT EXIST %BUILD_DRIVE%%BUILD_DIRECTORY% MKDIR %BUILD_DRIVE%%BUILD_DIRECTORY%
SET LOGFILE=%BUILD_DRIVE%%BUILD_DIRECTORY%\InputFunnelSDK.log
nmake /F InputFunnelSDK.mak BuildConfig="Win32 Release" ProductRootDirName="%PRODUCT_ROOT%" DoEverything 2>&1 | tee "%LOGFILE%"
IF %ERRORLEVEL% == 0 GOTO success
GOTO failure

:success
SET EMAIL_SUBJECT=InputFunnelSDK build successful!    :-)
GOTO done

:failure
SET EMAIL_SUBJECT=InputFunnelSDK build failed!    :-(
GOTO done


:done
