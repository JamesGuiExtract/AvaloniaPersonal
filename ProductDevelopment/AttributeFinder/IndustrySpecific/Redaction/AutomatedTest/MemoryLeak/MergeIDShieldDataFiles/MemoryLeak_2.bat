:: Clean Source folder
if exist "%testpath%\Source\" (
del /q "%testpath%\Source\*.*"
) else (
md "%testpath%\Source"
)

:: Wait a few seconds
"%ccdir%\sleep" 10s

:: Supply Numbered Files for hours specified in runtest.bat
start "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%testpath%\Images\TestImage001.ssn.voa" "%testpath%\Source" 100ms -h%processingtime%
start "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%testpath%\Images\TestImage001.dob.voa" "%testpath%\Source" 100ms -h%processingtime%
start "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%testpath%\Images\TestImage001.tif" "%testpath%\Source" 100ms -h%processingtime%
start "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%testpath%\Images\TestImage008.ssn.voa" "%testpath%\Source" 100ms -h%processingtime%
start "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%testpath%\Images\TestImage008.dob.voa" "%testpath%\Source" 100ms -h%processingtime%
start "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%testpath%\Images\TestImage008.tif" "%testpath%\Source" 100ms -h%processingtime%
