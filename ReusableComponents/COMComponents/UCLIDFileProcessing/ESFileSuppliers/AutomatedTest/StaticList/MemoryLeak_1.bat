:: Clean Source folder
if exist "%testpath%\Source\" (
del /q "%testpath%\Source\*.*"
) else (
md "%testpath%\Source"
)

:: Wait a few seconds
"%ccdir%\sleep" 10s

:: Supply Numbered Files for hours specified in runtest.bat
for %%i in (One Two Three Four Five Six) DO (
start  /B "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%testpath%\dummy.tif" "%testpath%\Source\%%i" 0ms -n10000
)
