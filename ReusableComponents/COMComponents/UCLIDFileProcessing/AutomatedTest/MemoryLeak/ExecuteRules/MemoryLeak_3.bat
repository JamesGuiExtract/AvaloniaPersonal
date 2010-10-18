:: Clean Source folder
if exist "%testpath%\Source\" (
del /q "%testpath%\Source\*.*"
) else (
md "%testpath%\Source"
)

:: Wait a few seconds
"%ccdir%\sleep" 10s

:: Supply Numbered Files for hours specified in runtest.bat
start "CopyNumberedFiles" "%ccdir%\CopyNumberedFiles.exe" "%ccdir%\..\TestingFiles\ProductDevelopment\AttributeFinder\AFCore\AutomatedTest\Images\Image2.tif" "%testpath%\Source" 2 -h%processingtime%
