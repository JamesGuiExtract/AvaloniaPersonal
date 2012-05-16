@ECHO OFF
CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

REM Get specified version of files from Common dir as well as IcoMapESRI\build dir
CD ..\..\Common
ss get $/Engineering/ProductDevelopment/Common -R -I- -W -V%1
CD ..\IcoMapESRI\Build
ss get $/Engineering/ProductDevelopment/IcoMapESRI/Build -R -I- -W -V%1

Rem Remove previous build directory if it exists
IF EXIST %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT% RMDIR /S /Q %BUILD_DRIVE%%BUILD_DIRECTORY%\%PRODUCT_ROOT%

IF NOT EXIST %BUILD_DRIVE%%BUILD_DIRECTORY% MKDIR %BUILD_DRIVE%%BUILD_DIRECTORY%
SET LOGFILE=%BUILD_DRIVE%%BUILD_DIRECTORY%\IcoMapESRI.log
nmake /F IcoMapESRISetup.mak BuildConfig="Release" ProductRootDirName="%PRODUCT_ROOT%" DoEverything 2>&1 | tee "%LOGFILE%"
IF %ERRORLEVEL% == 0 GOTO success
GOTO failure

:success
SET EMAIL_SUBJECT=IcoMapESRI build successful!    :-)
GOTO done

:failure
SET EMAIL_SUBJECT=IcoMapESRI build failed!    :-(
GOTO done

:done
