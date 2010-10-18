ECHO OFF

REM First param = the folder to copy images from
SET source=%1
Set source=%source:""="%

REM Second param = the folder to copy images to
SET dest=%2
Set dest=%dest:""="%

REM Third param = the frequency in which the full set from source will
REM be copied to dest
SET freq=%3

REM Fourth param = the number of hours to run.  This is approximate.  The actual
REM time run will be this parameter + the time required to copy the files
SET /a count=%4*60*60/%freq%

SET i=1

:LOOP
REM Master Loop... copy all tif/voa pairs from source every %freq% seconds
CALL :COPYSET %i%
SET /a i=%i%+1
IF %i%==%count% GOTO :EOF
CALL :WAIT %freq%
goto :LOOP

:COPYSET
REM Make a single numbered copy of the set in %source% to %dest%
FOR %%f IN (%source%\*.tif,%source%\*.jpg,%source%\*.gif,%source%\*.bmp,%source%\*.pdf) DO CALL :COPYFILES %%~nf %1
ECHO Set %1 of %count% copied.
:GOTO :EOF

:COPYFILES
REM Copy the tif\uss\voa set from source to target
IF EXIST %source%\%1.tif COPY %source%\%1.tif %dest%\copy%2_%1.tif > NUL
IF EXIST %source%\%1.tif.voa COPY %source%\%1.tif.voa %dest%\copy%2_%1.tif.voa > NUL
IF EXIST %source%\%1.tif.uss COPY %source%\%1.tif.uss %dest%\copy%2_%1.tif.uss > NUL
IF EXIST %source%\%1.jpg COPY %source%\%1.jpg %dest%\copy%2_%1.jpg > NUL
IF EXIST %source%\%1.jpg.voa COPY %source%\%1.jpg.voa %dest%\copy%2_%1.jpg.voa > NUL
IF EXIST %source%\%1.jpg.uss COPY %source%\%1.jpg.uss %dest%\copy%2_%1.jpg.uss > NUL
IF EXIST %source%\%1.gif COPY %source%\%1.gif %dest%\copy%2_%1.gif > NUL
IF EXIST %source%\%1.gif.voa COPY %source%\%1.gif.voa %dest%\copy%2_%1.gif.voa > NUL
IF EXIST %source%\%1.gif.uss COPY %source%\%1.gif.uss %dest%\copy%2_%1.gif.uss > NUL
IF EXIST %source%\%1.bmp COPY %source%\%1.bmp %dest%\copy%2_%1.bmp > NUL
IF EXIST %source%\%1.bmp.voa COPY %source%\%1.bmp.voa %dest%\copy%2_%1.bmp.voa > NUL
IF EXIST %source%\%1.bmp.uss COPY %source%\%1.bmp.uss %dest%\copy%2_%1.bmp.uss > NUL
IF EXIST %source%\%1.pdf COPY %source%\%1.pdf %dest%\copy%2_%1.pdf > NUL
IF EXIST %source%\%1.pdf.voa COPY %source%\%1.pdf.voa %dest%\copy%2_%1.pdf.voa > NUL
IF EXIST %source%\%1.pdf.uss COPY %source%\%1.pdf.uss %dest%\copy%2_%1.pdf.uss > NUL
GOTO :EOF

:WAIT
REM Hack to wait specified number of seconds
@ping 127.0.0.1 -n %1% -w 1000 > NUL
@ping 127.0.0.1 -n 1 -w 1000 > NUL
GOTO :EOF