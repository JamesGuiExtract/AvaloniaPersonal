@echo off
setlocal
::
:: get the correct program files directory for 32/64-bit OS
if defined programfiles(x86) set programfiles=%programfiles(x86)%

@echo Installing Extract Systems NetDMS exporter...
@echo.

:: get directory to patch files
set PatchDir=%~dp0

:: set directories
set PatchExporter=%PatchDir%\Exporter
set NetDMSConfigDir=%programfiles%\VistaSG\WorkFlow Configuration Manager
set NetDMSServerDir=%programfiles%\VistaSG\WorkFlow Server\Package

:: Copy Extract.NetDMSExporter.dll into the appropriate directories
if exist "%NetDMSConfigDir%" copy "%PatchExporter%\Extract.NetDMSExporter.dll" "%NetDMSConfigDir%"
if exist "%NetDMSServerDir%" copy "%PatchExporter%\Extract.NetDMSExporter.dll" "%NetDMSServerDir%"

@echo Extract Systems NetDMS exporter installation complete.
@echo.

endlocal
pause
goto :eof


