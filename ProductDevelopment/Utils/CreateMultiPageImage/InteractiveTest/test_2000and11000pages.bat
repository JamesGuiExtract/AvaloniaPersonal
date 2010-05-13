@echo off
setlocal
if not exist Page1.tif GOTO FILE_MISSING

:: ensure the testing folders exist
if not exist 2000Pages md 2000Pages
if not exist 11000Pages md 11000Pages

:: get the correct program files directory for 32/64-bit OS
if defined programfiles(x86) set programfiles=%programfiles(x86)%

:: set the path to CreateMultipageImage
set createMulti="%programfiles%\Extract Systems\CommonComponents\CreateMultiPageImage.exe"

:: clear the testing folders
del /Q .\2000Pages\*
del /Q .\11000Pages\*

echo Preparing to copy files

:: Copy 2000 image pages
set i=1
set name=2000Pages.
:COPY_2000
set fileName=%name%000%i%
if %i% GTR 9 set fileName=%name%00%i%
if %i% GTR 99 set fileName=%name%0%i%
if %i% GTR 999 set fileName=%name%%i%
copy Page1.tif .\2000Pages\%fileName%
set /a i+=1
if %i% LEQ 2000 GOTO COPY_2000

:: Copy 11000 image pages
set i=1
set name=11000Pages.
:COPY_11000
set fileName=%name%0000%i%
if %i% GTR 9 set fileName=%name%000%i%
if %i% GTR 99 set fileName=%name%00%i%
if %i% GTR 999 set fileName=%name%0%i%
if %i% GTR 9999 set fileName=%name%%i%
copy Page1.tif .\11000Pages\%fileName%
set /a i+=1
if %i% LEQ 11000 GOTO COPY_11000

echo File copying complete, preparing to run tests

echo Testing 2000 pages
%createMulti% .\2000Pages
if not exist .\2000Pages\2000Pages.tif GOTO FAILED

echo Testing 11000 pages
%createMulti% .\11000Pages
if not exist .\11000Pages\11000Pages.tif GOTO FAILED

echo Testing completed successfully

GOTO END_BATCH

:FILE_MISSING
echo Page1.tif not found, exiting
goto END_BATCH

:FAILED
echo Test failed
goto END_BATCH

:END_BATCH
pause
