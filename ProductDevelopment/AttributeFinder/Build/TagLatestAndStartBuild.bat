"C:\Program Files\PuTTY\pageant.exe" C:\Users\Product_builder\.ssh\pb.ppk

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

SET BUILD_STATUS="Started"

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%~p0..\..\Common\PowerShell\SendBuildStatuseMail.ps1' '%VersionToBuild%' '%BUILD_STATUS%'"

cd "%~p0"

call AttributeFinderSDK.bat "%VersionToBuild%"
IF %ERRORLEVEL% NEQ 0 (
	Echo AttributeFinderSDK exited with error.
	Echo.
	goto ExitWithError
)

goto Exit_Batch

:ExitWithError

SET BUILD_STATUS="Failed"

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%~p0..\..\Common\PowerShell\SendBuildStatuseMail.ps1' ' %VersionToBuild% that was started' '%BUILD_STATUS%'"

:Exit_Batch

cd "%~p0"

