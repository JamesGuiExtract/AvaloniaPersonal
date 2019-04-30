CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat


IF NOT EXIST %BUILD_DRIVE%%BUILD_DIRECTORY% MKDIR %BUILD_DRIVE%%BUILD_DIRECTORY%
SET TAGLOGFILE=%BUILD_DRIVE%%BUILD_DIRECTORY%\TagVersion.log

if "%~1"=="" (
	SET Branch=master
) else (
	SET Branch=%~1
)

Set GitPath="C:\Program Files\Git\bin\git.exe"
d:

cd %BUILD_VSS_ROOT%\Engineering

%GitPath% checkout  -f  %Branch% 2>&1 | tee "%TAGLOGFILE%"
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to Checkout %Branch%
	Echo.
	goto ExitWithError
)

%GitPath% pull origin -f %Branch% 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to pull changes from origin %Branch%
	Echo.
	goto ExitWithError
)
%GitPath% clean -d -f -x 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to clean %Branch%
	Echo.
	goto ExitWithError
)
cd %BUILD_VSS_ROOT%\Engineering\RC.Net\APIs

%GitPath% checkout  -f  %Branch% 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to Checkout %Branch%
	Echo.
	goto ExitWithError
)

%GitPath% pull origin -f %Branch% 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to pull changes from origin %Branch%
	Echo.
	goto ExitWithError
)
%GitPath% clean -d -f -x 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to clean %Branch%
	Echo.
	goto ExitWithError
)
cd %BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs 

%GitPath% checkout  -f  %Branch% 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to Checkout %Branch%
	Echo.
	goto ExitWithError
)

%GitPath% pull origin -f %Branch% 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to pull changes from origin %Branch%
	Echo.
	goto ExitWithError
)
%GitPath% clean -d -f -x 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to clean %Branch%
	Echo.
	goto ExitWithError
)
cd %BUILD_VSS_ROOT%\Engineering\Rules

%GitPath% checkout  -f  %Branch% 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to Checkout %Branch%
	Echo.
	goto ExitWithError
)

%GitPath% pull origin -f %Branch% 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to pull changes from origin %Branch%
	Echo.
	goto ExitWithError
)
%GitPath% clean -d -f -x 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to clean %Branch%
	Echo.
	goto ExitWithError
)

cd %BUILD_VSS_ROOT%\Engineering\ProductDevelopment\Common

cscript IncrementBuildVersion.vbs LatestComponentVersions.mak 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to update version
	Echo.
	goto ExitWithError
)

cd %BUILD_VSS_ROOT%\Engineering\Rules\Build_FKB

FOR /F "tokens=2 delims==" %%F IN ('findstr FKBVersion FKBVersion.mak') DO SET LAST_FKB_TAG=%%F
SET LAST_FKB_TAG=%LAST_FKB_TAG: Ver. =/%
SET LAST_FKB_TAG=%LAST_FKB_TAG:.=/%

SET FKBBuildNeeded=False

SET PATH=%PATH%;;C:\Program Files\Git\bin\

FOR /F "tokens=*" %%F IN ('git log -n 1 --format^=format:"True" "%LAST_FKB_TAG%"..HEAD -- ..\ComponentData') DO (
	SET FKBBuildNeeded=%%F
)

IF "%FKBBuildNeeded%"=="True" (
	cscript IncrementBuildVersion.vbs FKBVersion.mak 2>&1 | tee "%TAGLOGFILE%" -Append
	IF %ERRORLEVEL% NEQ 0 (
		Echo Unable to update version
		Echo.
		goto ExitWithError
	)
    FOR /F "tokens=2 delims==" %%F IN ('findstr FKBVersion FKBVersion.mak') DO SET FKBVersionToBuild=%%F

    %GitPath% commit -a -m "%FKBVersionToBuild%" 2>&1 | tee "%TAGLOGFILE%" -Append
)

cd %BUILD_VSS_ROOT%\Engineering\ProductDevelopment\Common

:: Get the version to build from the LatestComponentVersion.mak files
for /F "tokens=2 delims==" %%i in ( 'findstr FlexIndex LatestComponentVersions.mak') do set VersionToBuild=%%i

@ECHO FKBBuildNeeded = %FKBBuildNeeded%

%GitPath% commit -a -m "%VersionToBuild%" 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to commit new version
	Echo.
	goto ExitWithError
)

IF EXIST "%TEMP%\nmakeErrors" del "%TEMP%\nmakeErrors"
nmake -F TagFromLatest.mak FKBBuildNeeded=%FKBBuildNeeded%   TagRepos 2>&1 | tee "%TAGLOGFILE%" -Append
IF EXIST "%TEMP%\nmakeErrors" (
	FIND "NMAKE : fatal error" "%TEMP%\nmakeErrors"
:: If there were no errors nothing will be found and FIND will return an errorlevel of 1
	IF NOT ERRORLEVEL 1 (
		SET BUILD_STATUS="Failed"
		GOTO ExitWithError
	)
)

:: Push the changes with tags to the shared repos
cd %BUILD_VSS_ROOT%\Engineering\Rules
%GitPath% push origin --tags %Branch% 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to push Rules %Branch% to origin
	Echo.
	goto ExitWithError
)

cd %BUILD_VSS_ROOT%\Engineering\RC.Net\APIs
%GitPath% push origin --tags %Branch% 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to push RC.Net\APIs %Branch% to origin
	Echo.
	goto ExitWithError
)

cd %BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs 
%GitPath% push origin --tags %Branch% 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to push ReusableComponents\APIs %Branch% to origin
	Echo.
	goto ExitWithError
)

cd %BUILD_VSS_ROOT%\Engineering
%GitPath% push origin --tags %Branch% 2>&1 | tee "%TAGLOGFILE%" -Append
IF %ERRORLEVEL% NEQ 0 (
	Echo Unable to push Engineering %Branch% to origin
	Echo.
	goto ExitWithError
)

:: Rename the TagLogFile to have version
REN "%TAGLOGFILE% "%VersionToBuild% %TAGLOGFILE%"

:ExitWithoutError
EXIT /B 0

:ExitWithError
EXIT /B 1
