@echo off
::This batch file will step through all subdirectories of the parent folder and register all C++ files 
::listed in files with the extension rl & all .Net assemblies listed in files with the extension nl

::Set path so CommonComponents is on the path (assumes these files are in commoncomponents directory)
set PATH=%PATH%;%~p0;%~p0..\FlexIndexComponents\Bin;%~p0..\InputFunnelComponents\Bin;%~p0..\IcoMap for ArcGIS\Bin

if not [%1]==[/s] (
	echo Press any key to start file registration process...
	pause > nul
	echo.
)

for /R .\ %%r in (*.rl,*.nl) do (
	for /F "tokens=1 delims=," %%i in (%%~sr) do (
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
			"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\regasm" "%%~dpr%%i" /silent
		)
	)
)

if not [%1]==[/s] (
	echo.
	echo File registration process complete. Press any key to continue...
	pause > nul

)