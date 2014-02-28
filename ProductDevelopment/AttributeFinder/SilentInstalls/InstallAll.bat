@ECHO OFF

@ECHO Uninstalling all Extract Systems applications...
call "%~dp0UninstallExtract.bat"

IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	set LABDE_ISS="%~dp0LabDE64.iss"
	set FLEXINDEX_ISS="%~dp0FlexIndex64.iss"
	set IDSHIELD_ISS="%~dp0Idshield64.iss"
	set EXTRACT_COMMON=C:\Program Files (x86^)\Extract Systems\CommonComponents
) ELSE (
	set LABDE_ISS="%~dp0LabDE.iss"
	set FLEXINDEX_ISS="%~dp0FlexIndex.iss"
	set IDSHIELD_ISS="%~dp0Idshield.iss"
	set EXTRACT_COMMON=C:\Program Files\Extract Systems\CommonComponents
)
SET LM_ISS="%~dp0LM.iss"

@ECHO.
@ECHO Installing LabDE
start /wait "" "%~dp0..\LabDE\Setup" /s /w /f1%LABDE_ISS% /f2nul

@ECHO.
@ECHO Installing IDShield
start /wait "" "%~dp0..\IDShield\Setup" /s /w /f1%IDSHIELD_ISS% /f2nul

@ECHO.
@ECHO Installing FlexIndex
start /wait "" "%~dp0..\FlexIndex\Setup" /s /w /f1%FLEXINDEX_ISS% /f2nul

@ECHO.
@ECHO Installing Extract Systems LM
start /wait "" "%~dp0..\Extract Systems LM\Setup" /s /w /f1%LM_ISS% /f2nul

call "%EXTRACT_COMMON%\RegisterAll.bat" /s

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
@ECHO.
@PAUSE
