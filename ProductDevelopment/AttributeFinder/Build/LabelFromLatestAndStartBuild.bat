CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

SET Branch=
SET VersionToBuild=

:: This should be called with either no arguments indicating build from main branch
:: 	or have one argument that is the branch to build
if "%~1"=="" GOTO get_latest

SET Branch=%~1

:get_latest

:: Get build folders from vault to make sure they are the most current
vault GET -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% -nonworkingfolder "%~dp0..\..\Common" "$%Branch%/Engineering/ProductDevelopment/Common"
CD ..\AttributeFinder\Build
vault GET -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% -nonworkingfolder "%~dp0" "$%Branch%/Engineering/ProductDevelopment/AttributeFinder/Build"

cd "%~p0..\..\Common"

cscript IncrementBuildVersion.vbs

:: Commit the modified LatestComponentVersions.mak file
vault COMMIT -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% "$%Branch%/Engineering/ProductDevelopment/Common/LatestComponentVersions.mak"

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

