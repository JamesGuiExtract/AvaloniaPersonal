:: Clean Source and Destination folders
if exist "%testpath%\Source\" (
del /q "%testpath%\Source\*.*"
) else (
md "%testpath%\Source"
)
if exist "%testpath%\Destination\" (
del /q "%testpath%\Destination\*.*"
) else (
md "%testpath%\Destination"
)

:: Ensure Sample.txt is readonly
attrib +R "%testpath%\Sample.txt"

:: Wait a few seconds
"%ccdir%\sleep" 10s

:: Supply Numbered Files for hours specified in runtest.bat
start "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%testpath%\sample.txt" "%testpath%\Source" 1 -h%processingtime%
