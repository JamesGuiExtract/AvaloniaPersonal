@echo off

:: Compare the xml output files
fc .\Image1.tif.v1spatial.xml .\ExpectedOutput\Image1.tif.v1spatial.xml
fc .\Image1.tif.v1nospatial.xml .\ExpectedOutput\Image1.tif.v1nospatial.xml
fc .\Image1.tif.v2spatial.xml .\ExpectedOutput\Image1.tif.v2spatial.xml
fc .\Image1.tif.v2nospatial.xml .\ExpectedOutput\Image1.tif.v2nospatial.xml
fc .\test6.xml .\ExpectedOutput\test6.xml
fc .\test7.xml .\ExpectedOutput\test7.xml
fc .\test8.xml .\ExpectedOutput\test8.xml
echo Finished!
pause
