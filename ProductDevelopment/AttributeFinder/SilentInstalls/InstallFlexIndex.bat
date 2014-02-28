@ECHO OFF

@ECHO Uninstalling all Extract Systems applications...
call "%~dp0UninstallExtract.bat"

IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	set FLEXINDEX_ISS="%~dp0FlexIndex64.iss"
	set EXTRACT_COMMON=C:\Program Files (x86^)\Extract Systems\CommonComponents
) ELSE (
	set FLEXINDEX_ISS="%~dp0FlexIndex.iss"
	set EXTRACT_COMMON=C:\Program Files\Extract Systems\CommonComponents
)

@ECHO.
@ECHO Installing Flex Index...
start /wait "" "%~dp0..\FlexIndex\Setup" /s /f1%FLEXINDEX_ISS% /f2nul

call "%EXTRACT_COMMON%\RegisterAll.bat" /s

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
@ECHO.
@PAUSE
