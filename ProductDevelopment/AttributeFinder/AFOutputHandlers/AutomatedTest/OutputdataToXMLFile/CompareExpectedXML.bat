@echo off

:: Compare the xml output files
fc .\Image1.tif.v1spatial.xml .\ExpectedOutput\Image1.tif.v1spatial.xml
fc .\Image1.tif.v1nospatial.xml .\ExpectedOutput\Image1.tif.v1nospatial.xml
fc .\Image1.tif.v2spatial.xml .\ExpectedOutput\Image1.tif.v2spatial.xml
fc .\Image1.tif.v2nospatial.xml .\ExpectedOutput\Image1.tif.v2nospatial.xml
echo Finished!
pause
