:: Clean Source folder
if exist "%testpath%\Source\" (
del /q "%testpath%\Source\*.*"
) else (
md "%testpath%\Source"
)

:: Wait a few seconds
"%ccdir%\sleep" 10s

:: Supply Numbered Files for hours specified in runtest.bat
start "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%testpath%\Image1.pdf.voa" "%testpath%\Source" 5s -h%processingtime%
start "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%testpath%\Image1.pdf" "%testpath%\Source" 5s -h%processingtime%
