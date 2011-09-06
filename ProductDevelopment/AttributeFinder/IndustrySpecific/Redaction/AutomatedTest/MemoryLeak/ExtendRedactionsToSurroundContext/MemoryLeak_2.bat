:: Clean Source folder
if exist "%testpath%\Source\" (
del /q "%testpath%\Source\*.*"
) else (
md "%testpath%\Source"
)
if exist "%testpath%\VOA\" (
del /q "%testpath%\VOA\*.*"
) else (
md "%testpath%\VOA"
)

:: Wait a few seconds
"%ccdir%\sleep" 10s

:: Supply Numbered Files for hours specified in runtest.bat
start "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%testpath%\Image.tif.voa" "%testpath%\VOA" 1 -h%processingtime%
start "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%testpath%\Image.tif.uss" "%testpath%\Source" 1 -h%processingtime%
start "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%testpath%\Image.tif" "%testpath%\Source" 1 -h%processingtime%
