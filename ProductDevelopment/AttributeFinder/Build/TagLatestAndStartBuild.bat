CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

CALL %BUILD_VSS_ROOT%\Engineering\ProductDevelopment\Common\TagLatest.bat %~1
IF %ERRORLEVEL% NEQ 0 (
	Echo TagLatest exited with error.
	Echo.
	goto ExitWithError
)

:: Get the version to build from the LatestComponentVersion.mak files
pushd "%~p0..\..\Common"
for /F "tokens=2 delims==" %%i in ( 'findstr FlexIndex LatestComponentVersions.mak') do set VersionToBuild=%%i
popd

SET BUILD_STATUS=Tagged

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%~p0..\..\Common\PowerShell\SendBuildStatuseMail.ps1' '%VersionToBuild%' '%BUILD_STATUS%'"

cd "%~p0"

call AttributeFinderSDK.bat "%VersionToBuild%"


:Exit_Batch

cd "%~p0"

