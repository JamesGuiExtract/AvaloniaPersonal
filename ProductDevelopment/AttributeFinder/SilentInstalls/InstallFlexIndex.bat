@ECHO OFF

@ECHO Uninstalling all Extract Systems applications...
call "%~dp0UninstallExtract.bat"

:: Setup the paths for the installs
:: Assumes the internal build location is in SilentInstalls parallel to all the product installs
:: Assumes release build location has a Other\SilentInstalls folder with the product installs
::		in a folder parallel to Other and the product folder has the SetupFiles folder which is
::		the same as the internal build root folder
SET FLEXINDEX_ROOT=%~dp0

:: Replaces the Other\SilentInstalls path if it exists with the Release path 
CALL SET FLEXINDEX_ROOT=%FLEXINDEX_ROOT:Other\SilentInstalls=FLEXIndex\SetupFiles%

:: If replace the SilentInstalls with the product folder 
CALL SET FLEXINDEX_ROOT=%FLEXINDEX_ROOT:SilentInstalls=FLEXIndex%


IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	set FLEXINDEX_ISS="%~dp0FlexIndex64.iss"
	set EXTRACT_COMMON=C:\Program Files (x86^)\Extract Systems\CommonComponents
	set FLEXINDEX_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{A7DFE34D-A07E-4D57-A624-B758E42A69D4}"
) ELSE (
	set FLEXINDEX_ISS="%~dp0FlexIndex.iss"
	set EXTRACT_COMMON=C:\Program Files\Extract Systems\CommonComponents
	set FLEXINDEX_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{A7DFE34D-A07E-4D57-A624-B758E42A69D4}"
)

@ECHO.
@ECHO Installing Flex Index...
start /wait "" "%FLEXINDEX_ROOT%Setup" /s /f1%FLEXINDEX_ISS% /f2nul

:: Check registry for the uninstall for FlexIndex as verification that it installed
IF EXIST "%TEMP%\FlexIndexInstalled.reg" DEL "%TEMP%\FlexIndexInstalled.reg"
@regedit /e "%TEMP%\FlexIndexInstalled.reg" %FLEXINDEX_KEY%
IF NOT EXIST "%TEMP%\FlexIndexInstalled.reg" (
	@ECHO There was a error installing FLEX Index
	GOTO END
)
DEL "%TEMP%\FlexIndexInstalled.reg"

call "%EXTRACT_COMMON%\RegisterAll.bat" /s

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
:END
@ECHO.
@PAUSE
