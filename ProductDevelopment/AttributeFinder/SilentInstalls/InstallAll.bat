@ECHO OFF

setlocal

%~d0
cd %~p0

@ECHO Uninstalling all Extract Systems applications...
start /wait "" ..\ExtractUninstaller\ExtractUninstaller /s /f2nul

IF EXIST "C:\Program Files (x86)" (
	set LABDE_ISS="%~dp0LabDE64.iss"
	set FLEXINDEX_ISS="%~dp0FlexIndex64.iss"
	set IDSHIELD_ISS="%~dp0Idshield64.iss"
) ELSE (
	set LABDE_ISSFILE="%~dp0LabDE.iss"
	set FLEXINDEX_ISS="%~dp0FlexIndex.iss"
	set IDSHIELD_ISS="%~dp0Idshield.iss"
)
SET LM_ISS="%~dp0LM.iss"

@ECHO.
@ECHO Installing LabDE
start /wait "" ..\LabDE\Setup /s /w /f1%LABDE_ISS% /f2nul
f
@ECHO.
@ECHO Installing IDShield
start /wait "" ..\IDShield\Setup /s /w /f1%IDSHIELD_ISS% /f2nul

@ECHO.
@ECHO Installing FlexIndex
start /wait "" ..\FlexIndex\Setup /s /w /f1%FLEXINDEX_ISS% /f2nul

@ECHO.
@ECHO Installing Extract Systems LM
start /wait "" "..\Extract Systems LM\Setup" /s /w /f1%LM_ISS% /f2nul

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
@ECHO.
@PAUSE
