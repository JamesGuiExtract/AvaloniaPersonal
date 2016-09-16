CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat


SET BUILD_STATUS="Started"

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%~p0..\..\Common\PowerShell\SendBuildStatuseMail.ps1' '%VersionToBuild%' '%BUILD_STATUS%'"

CALL %BUILD_VSS_ROOT%\Engineering\ProductDevelopment\Common\TagLatest.bat
IF %ERRORLEVEL% NEQ 0 (
	Echo Vault exited with error.
	Echo.
	goto ExitWithError
)

cd "%~p0"

call AttributeFinderSDK.bat "%VersionToBuild%"
IF %ERRORLEVEL% NEQ 0 (
	Echo Vault exited with error.
	Echo.
	goto ExitWithError
)

goto Exit_Batch

:ExitWithError

SET BUILD_STATUS="Failed"

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%~p0..\..\Common\PowerShell\SendBuildStatuseMail.ps1' ' %VersionToBuild% that was started' '%BUILD_STATUS%"

:Exit_Batch

cd "%~p0"

