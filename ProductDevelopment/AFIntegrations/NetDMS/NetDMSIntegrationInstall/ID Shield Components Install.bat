@echo off
setlocal
::
:: get the correct program files directory for 32/64-bit OS
if defined programfiles(x86) set programfiles=%programfiles(x86)%

@echo Installing NetDMS integration ...
@echo.

:: get directory to patch files
set PatchDir=%~dp0

:: set directories
set PatchProgramFiles=%PatchDir%\ProgramFiles
set ExtractComponentsDir=%programfiles%\Extract Systems\CommonComponents
set NetDMSConfigDir=%programfiles%\VistaSG\WorkFlow Configuration Manager
set NetDMSServerDir=%programfiles%\VistaSG\WorkFlow Server\Package

:: Copy all files from the Patch\ProgramFiles directory to CommonComponents
xcopy /Y /Q "%PatchProgramFiles%\*.*" "%ExtractComponentsDir%\"

:: Copy Regasm into CommonComponents so that a special config file can be applied.
copy "C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe.*" "%ExtractComponentsDir%" > NUL
@echo.

:: Register Extract.NetDMSCustomComponents.dll
@echo Registering NetDMS integration components...
"%ExtractComponentsDir%\RegAsm.exe" "%ExtractComponentsDir%\Extract.NetDMSCustomComponents.dll"
del /Q "%ExtractComponentsDir%\RegAsm.exe.*"
@echo.

@echo NetDMS Integration update complete.
@echo Please open the FAM and select "Check for new components" from the tools menu to make the new FAM tasks available.
@echo.

endlocal
pause
goto :eof


