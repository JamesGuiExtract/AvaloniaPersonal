CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

SET Branch=
SET VersionToBuild=

:: This should be called with either no arguments indicating build from main branch
:: 	or have one argument that is the branch to build
if "%~1"=="" GOTO get_latest

SET Branch=%~1

:get_latest

:: Get rid of the trailing \ if it is there
SET BATCH_WORKING_FOLDER=%~dp0
IF %BATCH_WORKING_FOLDER:~-1%==\ SET BATCH_WORKING_FOLDER=%BATCH_WORKING_FOLDER:~0,-1%

SET BATCH_COMMON_PATH=$%Branch%/Engineering/ProductDevelopment/Common
SET BATCH_ATTRIBUTE_BUILD=$%Branch%/Engineering/ProductDevelopment/AttributeFinder/Build

cd "%~p0..\..\Common"
:: Get build folders from vault to make sure they are the most current
vault GET -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% -merge overwrite -workingfolder "%BATCH_WORKING_FOLDER%\..\..\Common" "%BATCH_COMMON_PATH%"

IF %ERRORLEVEL% NEQ 0 (
	Echo Vault exited with error.
	Echo.
	goto ExitWithError
)

CD "%~p0"
vault GET -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% -merge overwrite -workingfolder "%BATCH_WORKING_FOLDER%" "%BATCH_ATTRIBUTE_BUILD%"

IF %ERRORLEVEL% NEQ 0 (
	Echo Vault exited with error.
	Echo.
	goto ExitWithError
)

cd "%~p0..\..\Common"

cscript IncrementBuildVersion.vbs

:: Commit the modified LatestComponentVersions.mak file
vault COMMIT -server %VAULT_SERVER% -repository %VAULT_REPOSITORY% "%BATCH_COMMON_PATH%/LatestComponentVersions.mak"

IF %ERRORLEVEL% NEQ 0 (
	Echo Vault exited with error.
	Echo.
	goto ExitWithError
)

:: Label
nmake /F LabelFromLatestVersions.mak Branch=%Branch%

IF %ERRORLEVEL% NEQ 0 (
	Echo Labeling exited with error.
	Echo.
	goto ExitWithError
)

:: Get the version to build from the LatestComponentVersion.mak files
for /F "tokens=2 delims==" %%i in ( 'findstr FlexIndex LatestComponentVersions.mak') do set VersionToBuild=%%i

cd "%~p0"

::this needs the version from latest component versions
if "%Branch%"=="" (
	call AttributeFinderSDK.bat "%VersionToBuild%"
) else (
	call AttributeFinderSDK.bat "%VersionToBuild%" "%Branch%"
)

goto Exit_Batch

:ExitWithError

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '.\SendBuildStatuseMail.ps1' ' that was started' 'Failed'"

:Exit_Batch

cd "%~p0"

