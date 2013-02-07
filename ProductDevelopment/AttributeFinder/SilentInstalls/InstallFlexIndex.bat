@ECHO OFF

setlocal

%~d0
cd %~p0

@ECHO Uninstalling all Extract Systems applications...
start /wait "" ..\ExtractUninstaller\ExtractUninstaller /s

IF EXIST "C:\Program Files (x86)" (
	set FLEXINDEX_ISS="%~dp0FlexIndex64.iss"
) ELSE (
	set FLEXINDEX_ISS="%~dp0FlexIndex.iss"
)

SET LM_ISS="%~dp0LM.iss"

@ECHO.
@ECHO Installing Flex Index...
start /wait "" "..\FlexIndex\Setup" /s /f1%FLEXINDEX_ISS% /f2nul

@ECHO.
@ECHO Installing Extract Systems LM
start /wait "" "..\Extract Systems LM\Setup" /s /f1%LM_ISS% /f2nul

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
@ECHO.
@PAUSE
