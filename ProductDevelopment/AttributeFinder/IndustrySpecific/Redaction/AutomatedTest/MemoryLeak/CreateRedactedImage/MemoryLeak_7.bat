:: Clean Source folder
if exist "%testpath%\Source\" (
del /q "%testpath%\Source\*.*"
) else (
md "%testpath%\Source"
)

:: Wait a few seconds
"%ccdir%\sleep" 10s

:: Supply Numbered Files for hours specified in runtest.bat
start "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%testpath%\samplecolor.pdf.voa" "%testpath%\Source" 2s -h%processingtime%
start "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%testpath%\samplecolor.pdf" "%testpath%\Source" 2s -h%processingtime%
