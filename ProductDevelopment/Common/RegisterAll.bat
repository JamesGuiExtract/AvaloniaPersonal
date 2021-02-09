@echo off
::This batch file will step through all subdirectories of the parent folder and register all C++ files 
::listed in files with the extension rl & all .Net assemblies listed in files with the extension nl

::Set path so CommonComponents is on the path (assumes these files are in commoncomponents directory)
set PATH=%PATH%;%~p0

if not [%1]==[/s] (
	echo Press any key to start file registration process...
	pause > nul
	echo.
)

::Change current folder to common components folder
setlocal

SET PROGRAM_ROOT=%ProgramFiles(x86)%

IF NOT "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	SET PROGRAM_ROOT=%ProgramFiles%
)

pushd %PROGRAM_ROOT%\Extract Systems\CommonComponents

for /R .\ %%r in (*.rl,*.nl) do (
	for /F "tokens=1 delims=," %%i in (%%~nxr) do (
		if "%%~xr" == ".rl" (
			if not [%1]==[/s] (
				echo Registering %%~dpr%%i...
			)
			if "%%~xi" == ".exe" (
				"%%~dpr%%i" /RegServer
			) else if not "%%~xi" == ".rl" (
				regsvr32 "%%~dpr%%i" /s
			)
		) else if "%%~xr" == ".nl" (
			if not [%1]==[/s] (
				echo Registering %%~dpr%%i...
			)
			"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\regasm" "%%~dpr%%i" /silent /codebase
		)
	)
)

:: set DCOM permissions for FAMProcess
dcomperm -al {08463A92-A444-48AF-8822-693C4F6E1F08} set users permit level:l
dcomperm -aa {08463A92-A444-48AF-8822-693C4F6E1F08} set users permit level:l

:: Set DCOM permissions for SSOCR2
dcomperm -al {752139E2-5977-4AD2-9E26-BE3B9235524C} set users permit level:l
dcomperm -aa {752139E2-5977-4AD2-9E26-BE3B9235524C} set users permit level:l

popd

endlocal

if not [%1]==[/s] (
	echo.
	echo File registration process complete. Press any key to continue...
	pause > nul

)
