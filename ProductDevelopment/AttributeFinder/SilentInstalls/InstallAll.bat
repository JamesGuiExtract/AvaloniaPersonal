@ECHO OFF

@ECHO Uninstalling all Extract Systems applications...
call "%~dp0UninstallExtract.bat"

IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	set LABDE_ISS="%~dp0LabDE64.iss"
	set FLEXINDEX_ISS="%~dp0FlexIndex64.iss"
	set IDSHIELD_ISS="%~dp0Idshield64.iss"
	set EXTRACT_COMMON=C:\Program Files (x86^)\Extract Systems\CommonComponents
	set IDSHIELD_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{158160CD-7B55-462F-8477-7E18B2937D40}"
	set FLEXINDEX_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{A7DFE34D-A07E-4D57-A624-B758E42A69D4}"
	set LABDE_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{0E412937-E4FA-4737-A321-00AED69497C7}"
) ELSE (
	set LABDE_ISS="%~dp0LabDE.iss"
	set FLEXINDEX_ISS="%~dp0FlexIndex.iss"
	set IDSHIELD_ISS="%~dp0Idshield.iss"
	set EXTRACT_COMMON=C:\Program Files\Extract Systems\CommonComponents
	set IDSHIELD_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{158160CD-7B55-462F-8477-7E18B2937D40}"
	set FLEXINDEX_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{A7DFE34D-A07E-4D57-A624-B758E42A69D4}"
	set LABDE_KEY="HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{0E412937-E4FA-4737-A321-00AED69497C7}"
)
SET LM_ISS="%~dp0LM.iss"

@ECHO.
@ECHO Installing LabDE
start /wait "" "%~dp0..\LabDE\Setup" /s /w /f1%LABDE_ISS% /f2nul

:: Check registry for the uninstall for LabDE as verification that it installed
IF EXIST "%TEMP%\LabDEInstalled.reg" DEL "%TEMP%\LabDEInstalled.reg"
@regedit /e "%TEMP%\LabDEInstalled.reg" LABDE_KEY
IF NOT EXIST "%TEMP%\LabDEInstalled.reg" (
	@ECHO There was a error installing LabDE
	GOTO END
)
DEL "%TEMP%\LabDEInstalled.reg"

@ECHO.
@ECHO Installing IDShield
start /wait "" "%~dp0..\IDShield\Setup" /s /w /f1%IDSHIELD_ISS% /f2nul

:: Check registry for the uninstall for ID Shield as verification that it installed
IF EXIST "%TEMP%\IDShieldInstalled.reg" DEL "%TEMP%\IDShieldInstalled.reg"
@regedit /e "%TEMP%\IDShieldInstalled.reg" IDSHIELD_KEY
IF NOT EXIST "%TEMP%\IDShieldInstalled.reg" (
	@ECHO There was a error installing ID Shield
	GOTO END
)
DEL "%TEMP%\IDShieldInstalled.reg"

@ECHO.
@ECHO Installing FlexIndex
start /wait "" "%~dp0..\FlexIndex\Setup" /s /w /f1%FLEXINDEX_ISS% /f2nul

:: Check registry for the uninstall for FlexIndex as verification that it installed
IF EXIST "%TEMP%\FlexIndexInstalled.reg" DEL "%TEMP%\FlexIndexInstalled.reg"
@regedit /e "%TEMP%\FlexIndexInstalled.reg" FLEXINDEX_KEY
IF NOT EXIST "%TEMP%\FlexIndexInstalled.reg" (
	@ECHO There was a error installing FLEX Index
	GOTO END
)
DEL "%TEMP%\FlexIndexInstalled.reg"

@ECHO.
@ECHO Installing Extract Systems LM
start /wait "" "%~dp0..\Extract Systems LM\Setup" /s /w /f1%LM_ISS% /f2nul

call "%EXTRACT_COMMON%\RegisterAll.bat" /s

@ECHO.
@ECHO.
@ECHO PLEASE REBOOT YOUR COMPUTER.
:END
@ECHO.
@PAUSE
