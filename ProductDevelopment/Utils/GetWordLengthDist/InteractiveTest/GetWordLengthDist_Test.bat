@echo off
rem Cleanup output files
call clean.bat

rem Create subfolders and copy files for testing
call copyTestFiles.bat

rem Run tests
echo Creating output files
GetWordLengthDist .\testFiles\1.uss >.\testFiles\testOutput_1.out
GetWordLengthDist .\testFiles\1.uss.txt >.\testFiles\testOutput_2.out
GetWordLengthDist .\testFiles\ >.\testFiles\testOutput_3.out
GetWordLengthDist .\testFiles\*.txt >.\testFiles\testOutput_4.out
GetWordLengthDist .\testFiles\ /r >.\testFiles\testOutput_5.out
GetWordLengthDist .\testFiles\*.txt /r >.\testFiles\testOutput_6.out
GetWordLengthDist .\testFiles\listTest.lst /fl >.\testFiles\testOutput_7.out
echo Preparing to compare files
pause
echo comparing test 1
fc .\ExpectedOutput\testOutput_1.out .\testFiles\testOutput_1.out
pause
echo comparing test 2
fc .\ExpectedOutput\testOutput_2.out .\testFiles\testOutput_2.out
pause
echo comparing test 3
fc .\ExpectedOutput\testOutput_3.out .\testFiles\testOutput_3.out
pause
echo comparing test 4
fc .\ExpectedOutput\testOutput_4.out .\testFiles\testOutput_4.out
pause
echo comparing test 5
fc .\ExpectedOutput\testOutput_5.out .\testFiles\testOutput_5.out
pause
echo comparing test 6
fc .\ExpectedOutput\testOutput_6.out .\testFiles\testOutput_6.out
pause
echo comparing test 7
fc .\ExpectedOutput\testOutput_7.out .\testFiles\testOutput_7.out
pause
echo Testing csv file output
GetWordLengthDist .\testFiles\ /oc >.\temp_csv.out
del .\temp_csv.out
dir /S .\testFiles\*.csv >.\temp.out
findstr /C:"6 File(s)" .\temp.out >.\find_6csv.out
del .\temp.out
fc .\ExpectedOutput\find_6csv.out .\find_6csv.out
del .\find_6csv.out
pause
GetWordLengthDist .\testFiles\ /r /oc >.\temp_csv.out
del .\temp_csv.out
dir /S .\testFiles\*.csv >.\temp.out
findstr /C:"24 File(s)" .\temp.out >.\find_24csv.out
del .\temp.out
fc .\ExpectedOutput\find_24csv.out .\find_24csv.out
del .\find_24csv.out
pause
echo Preparing to test error conditions
pause
echo Confirm usage is displayed
GetWordLengthDist
pause
echo Confirm invalid command line argument error and usage displayed
GetWordLengthDist .\testFiles\1.uss /g
pause
echo Confirm /ef must have a file error and usage displayed
GetWordLengthDist .\testFiles\1.uss /ef
pause
echo Confirm file does not exist exception
GetWordLengthDist .\testFiles\ThisFileDoesNotExist.uss
pause
echo Confirm file testException.uex exists with current timestamp
GetWordLengthDist .\testFiles\BadFileList.lst /fl /ef .\testException.uex
pause
echo Confirm exception file must end in .uex exception displayed
GetWordLengthDist .\testFiles\1.uss /ef .\test.exp
pause
echo Confirm exception file must end in .uex exception displayed
GetWordLengthDist .\testFiles\1.uss /ef /r
pause
echo Testing complete!
pause
