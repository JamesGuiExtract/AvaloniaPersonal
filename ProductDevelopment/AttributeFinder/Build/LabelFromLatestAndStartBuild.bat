CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

SET Branch=
SET VersionToBuild=

:: This should be called with either no arguments indicating build from main branch
:: 	or have one argument that is the branch to build
if "%~1"=="" GOTO get_latest

SET Branch=%~1

:get_latest

IF "Branch"=="" (
	SET BATCH_COMMON_PATH=$/Engineering/ProductDevelopment/Common
	SET BATCH_ATTRIBUTE_BUILD=$/Engineering/ProductDevelopment/AttributeFinder/Build
) ELSE (
	SET BATCH_COMMON_PATH=$%Branch%/Engineering/ProductDevelopment/Common
	SET BATCH_ATTRIBUTE_BUILD=$%Branch%/Engineering/ProductDevelopment/AttributeFinder/Build
)

cd "%~p0..\..\Common"
:: Get build folders from vault to make sure they are the most current
vault GET -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% -merge overwrite -workingfolder "%~dp0..\..\Common" "%BATCH_COMMON_PATH%"
CD "%~p0"
vault GET -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% -merge overwrite -workingfolder "%~dp0" "%BATCH_ATTRIBUTE_BUILD%"

cd "%~p0..\..\Common"

cscript IncrementBuildVersion.vbs

:: Commit the modified LatestComponentVersions.mak file
vault COMMIT -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% "$%BATCH_COMMON_PATH%/LatestComponentVersions.mak"

:: Label
nmake /F LabelFromLatestVersions.mak

:: Get the version to build from the LatestComponentVersion.mak files
for /F "tokens=2 delims==" %%i in ( 'findstr FlexIndex LatestComponentVersions.mak') do set VersionToBuild=%%i

cd "%~p0"

::this needs the version from latest component versions
if "%Branch%"=="" (
	call AttributeFinderSDK.bat "%VersionToBuild%"
) else (
	call AttributeFinderSDK.bat "%VersionToBuild%" "%Branch%"
)

