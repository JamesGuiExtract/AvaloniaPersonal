:: Clean Source folder
if exist "%testpath%\Source\" (
del /q "%testpath%\Source\*.*"
) else (
md "%testpath%\Source"
)

:: Wait a few seconds
"%ccdir%\sleep" 10s

:: Supply Numbered Files for hours specified in runtest.bat
start "CopyNumberedSets" "%testpath%\CopyNumberedSets" "%testpath%\AutomatedTestFiles\Images\AllImages" ""%testpath%\Source" 600 %processingtime%