@ECHO OFF

ECHO Please checkout $/Engineering/ProductDevelopment/Common/LatestComponentVersions.mak to make changes
PAUSE

REM Increment labels of dependent components
AutoIncrementVssLabel /p"$/Engineering/ReusableComponents"
AutoIncrementVssLabel /p"$/Engineering/ProductDevelopment/Utils"
AutoIncrementVssLabel /p"$/Engineering/ProductDevelopment/WindowsComponentsUpdate"
AutoIncrementVssLabel /p"$/Engineering/ProductDevelopment/UCLIDSoftwareInstallation"

ECHO Increment InputFunnel project's label...
AutoIncrementVssLabel /p"$/Engineering/ProductDevelopment/InputFunnel" /i
ECHO Please update LatestComponentVersions.mak...
pause

ECHO Please checkin $/Engineering/ProductDevelopment/Common/LatestComponentVersions.mak
PAUSE

REM Get latest LatestComponentVersions.mak and label the Common directory
ECHO Labeling $/Engineering/ProductDevelopment/Common directory ...
@ECHO OFF
CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

nmake /F InputFunnel.mak BuildConfig="Win32 Release" ProductRootDirName="%PRODUCT_ROOT%" LabelCommonFolder
