@ECHO OFF

@ECHO Uninstalling all Extract Systems applications...
start /wait "" "%~dp0..\ExtractUninstaller\ExtractUninstaller" /s /f2nul

IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	set FLEXINDEX_ISS="%~dp0FlexIndex64.iss"
) ELSE (
	set FLEXINDEX_ISS="%~dp0FlexIndex.iss"
)

@ECHO.
@ECHO Installing Flex Index...
start /wait "" "%~dp0..\FlexIndex\Setup" /s /f1%FLEXINDEX_ISS% /f2nul

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
@ECHO.
@PAUSE
