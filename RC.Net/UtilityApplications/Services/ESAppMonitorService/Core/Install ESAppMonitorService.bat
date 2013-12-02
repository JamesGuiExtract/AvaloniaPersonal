@ECHO OFF

@echo Installing Extract Systems Application Monitor Service...
@echo.

IF DEFINED programfiles(x86) SET programfiles=%programfiles(x86)%
set ExtractComponentsDir=%programfiles%\Extract Systems\CommonComponents

FOR /f "tokens=2*" %%a IN ('REG QUERY HKLM\SOFTWARE\Microsoft\.NETFramework /v InstallRoot') DO SET "DotNetPath=%%b"
SET DotNetPath=%DotNetPath:64=%v4.0.30319

"%DotNetPath%\InstallUtil.exe" "%ExtractComponentsDir%\ESAppMonitorService.exe"

@echo.
PAUSE