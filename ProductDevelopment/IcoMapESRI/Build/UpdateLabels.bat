@ECHO OFF

ECHO Please checkout $/Engineering/ProductDevelopment/Common/LatestComponentVersions.mak to make changes
PAUSE

REM Increment labels of dependent components
AutoIncrementVssLabel /p"$/Engineering/ProductDevelopment/PlatformSpecificUtils"
AutoIncrementVssLabel /p"$/Engineering/ReusableComponents"

ECHO Increment IcoMapCore project's label... 
AutoIncrementVssLabel /p"$/Engineering/ProductDevelopment/IcoMapCore" /i
ECHO Please update LatestComponentVersions.mak...
pause
ECHO Increment IcoMapESRI project's label... 
AutoIncrementVssLabel /p"$/Engineering/ProductDevelopment/IcoMapESRI" /i
ECHO Please update LatestComponentVersions.mak...
pause

ECHO Please checkin $/Engineering/ProductDevelopment/Common/LatestComponentVersions.mak
PAUSE

REM Get latest LatestComponentVersions.mak and label the Common directory
ECHO Labeling $/Engineering/ProductDevelopment/Common directory ...
@ECHO OFF
CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

nmake /F IcoMapESRISetup.mak BuildConfig="Win32 Release" ProductRootDirName="%PRODUCT_ROOT%" LabelCommonFolder

