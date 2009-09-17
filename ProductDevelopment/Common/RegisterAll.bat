@echo off
@REM This batch file will step through all subdirectories of the parent folder and register all files lsited
@REM in files with the extension rl
@REM Set path so CommonComponents is on the path ( assumes this files is in commoncomponents directory)
@SET PATH=%PATH%;%~p0;%~p0..\FlexIndexComponents\Bin;%~p0..\InputFunnelComponents\Bin;%~p0..\IcoMap for ArcGIS\Bin
@for /R ..\ %%r in (*.rl,*.nl) do (
	@for /F "tokens=1 delims=," %%i in (%%~sr) do (
		@if "%%~xr" == ".rl" (
			@if "%%~xi" == ".exe" (
				"%%~dpr%%i" /RegServer
			) else if not "%%~xi" == ".rl" (
				regsvr32 "%%~dpr%%i" /s
			)
		) else if "%%~xr" == ".nl" (
			"%WINDIR%\Microsoft.NET\Framework\v2.0.50727\regasm" "%%~dpr%%i" /silent
		)
	)
)
