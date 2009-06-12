@ECHO OFF

ECHO Please checkout $/Engineering/ProductDevelopment/Common/LatestComponentVersions.mak to make changes
PAUSE

ECHO Increment ReusableComponents project's label...
AutoIncrementVssLabel /p"$/Engineering/ReusableComponents" /i

ECHO Increment RC.Net project's label...
AutoIncrementVssLabel /p"$/Engineering/RC.Net" /i

ECHO Increment PD Utils project's label...
AutoIncrementVssLabel /p"$/Engineering/ProductDevelopment/Utils" /i

ECHO Increment PlatformSpecificUtils project's label...
AutoIncrementVssLabel /p"$/Engineering/ProductDevelopment/PlatformSpecificUtils" /i

ECHO Increment IcoMapCore project's label...
AutoIncrementVssLabel /p"$/Engineering/ProductDevelopment/IcoMapCore" /i

ECHO Increment IcoMapESRI project's label...
AutoIncrementVssLabel /p"$/Engineering/ProductDevelopment/IcoMapESRI" /i

ECHO Increment LF Integration project's label...
AutoIncrementVssLabel /p"$/Engineering/ProductDevelopment/AFIntegrations/Laserfiche" /i

ECHO Increment ID Shield Office label...
AutoIncrementVssLabel /p"$/Engineering/ProductDevelopment/IDShieldOffice" /i

ECHO Increment LabDE label...
AutoIncrementVssLabel /p"$/Engineering/ProductDevelopment/LabDE" /i

ECHO Increment AttributeFinder project's label...
AutoIncrementVssLabel /p"$/Engineering/ProductDevelopment/AttributeFinder" /i

ECHO Please update and checkin $/Engineering/ProductDevelopment/Common/LatestComponentVersions.mak
PAUSE

REM Get latest LatestComponentVersions.mak and label the Common directory
ECHO Labeling $/Engineering/ProductDevelopment/Common directory ...
@ECHO OFF
CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

nmake /F Common.mak ProductRootDirName="Common" BuildConfig="Release" LabelCommonDir
