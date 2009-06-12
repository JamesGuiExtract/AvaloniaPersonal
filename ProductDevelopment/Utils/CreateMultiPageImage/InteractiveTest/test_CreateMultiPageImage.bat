echo off

:: Make sure there is no a.tif file to start with
if exist a.tif del a.tif

if not exist Page1.tif GOTO FILE_MISSING
if not exist Page2.tif GOTO FILE_MISSING
if not exist Page3.tif GOTO FILE_MISSING

:: get the correct program files directory for 32/64-bit OS
if defined programfiles(x86) set programfiles=%programfiles(x86)%

:: set the path to CreateMultipageImage
set createMulti="%programfiles%\Extract Systems\CommonComponents\CreateMultiPageImage.exe"

:: Set up for test 1
:TEST_1
copy Page1.tif a.001.tif
copy Page2.tif a.002.tif
copy Page3.tif a.003.tif
echo Test 1
%createMulti% .\
if not exist a.tif GOTO FAILED

:: Set up for test 2
del a.tif
rename a.001.tif a.001
rename a.002.tif a.002
rename a.003.tif a.003
echo Test 2
%createMulti% .\
if not exist a.tif GOTO FAILED

:: Set up for test 3
del a.tif
rename a.001 a-1.tif
rename a.002 a-2.tif
rename a.003 a-3.tif
echo Test 3
%createMulti% .\
if not exist a.tif GOTO FAILED

:: Set up for test 4
del a.tif
rename a-1.tif a_P1.tif
rename a-2.tif a_P2.tif
rename a-3.tif a_P3.tif
echo Test 4
%createMulti% .\
if not exist a.tif GOTO FAILED

:: Set up for test 5
del a.tif
rename a_P1.tif a_0001.tif
rename a_P2.tif a_0002.tif
rename a_P3.tif a_0003.tif
echo Test 5
%createMulti% .\
if not exist a.tif GOTO FAILED

:: Set up for test 6
del a.tif
rename a_0001.tif 00000001.tif
rename a_0002.tif 00000002.tif
rename a_0003.tif 00000003.tif
echo Test 6
%createMulti% .\
if not exist a.tif GOTO FAILED

:: Cleanup the last remaining files
del a.tif
del 00000001.tif
del 00000002.tif
del 00000003.tif

GOTO EXIT

:FAILED
echo Current Test Failed!
pause
exit

:EXIT
echo Testing completed successfully!
pause
exit

:FILE_MISSING
echo Missing one of the test files Page1.tif, Page2.tif or Page3.tif
pause
exit
