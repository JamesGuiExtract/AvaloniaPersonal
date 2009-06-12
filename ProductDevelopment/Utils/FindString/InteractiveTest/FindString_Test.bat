@echo off
rem Cleanup output files
call clean.bat
cls

rem Create subfolders and copy files for testing
call copyTestFiles.bat

rem Create output files
echo Creating output files
pause
cls
echo Creating non-verbose output
FindString .\testFiles\1_1.uss "social" >.\testFiles\testOutput_1.out
FindString .\testFiles\1_1.txt "social" >.\testFiles\testOutput_2.out
FindString .\testFiles\1_1.uss "social" /c >.\testFiles\testOutput_3.out
FindString .\testFiles\1_1.txt "social" /c >.\testFiles\testOutput_4.out
FindString .\testFiles\*.uss "social" >.\testFiles\testOutput_5.out
FindString .\testFiles\*.txt "social" >.\testFiles\testOutput_6.out
FindString .\testFiles\*.uss "social" /c >.\testFiles\testOutput_7.out
FindString .\testFiles\*.txt "social" /c >.\testFiles\testOutput_8.out
FindString .\testFiles\1_1.uss "\d+-\d+-\d+" /e >.\testFiles\testOutput_9.out
FindString .\testFiles\1_1.txt "\d+-\d+-\d+" /e >.\testFiles\testOutput_10.out
FindString .\testFiles\1_1.uss "\d+-\d+-\d+" /c /e >.\testFiles\testOutput_11.out
FindString .\testFiles\1_1.txt "\d+-\d+-\d+" /c /e >.\testFiles\testOutput_12.out
FindString .\testFiles\*.uss "\d+-\d+-\d+" /e >.\testFiles\testOutput_13.out
FindString .\testFiles\*.txt "\d+-\d+-\d+" /e >.\testFiles\testOutput_14.out
FindString .\testFiles\*.uss "\d+-\d+-\d+" /c /e >.\testFiles\testOutput_15.out
FindString .\testFiles\*.txt "\d+-\d+-\d+" /c /e >.\testFiles\testOutput_16.out

echo Creating verbose output
FindString .\testFiles\1_1.uss "social" /v /vf .\testFiles\verboseOut_17.out >.\testFiles\testOutput_17.out
FindString .\testFiles\1_1.txt "social" /v /vf .\testFiles\verboseOut_18.out >.\testFiles\testOutput_18.out
FindString .\testFiles\1_1.uss "social" /c /v /vf .\testFiles\verboseOut_19.out >.\testFiles\testOutput_19.out
FindString .\testFiles\1_1.txt "social" /c /v /vf .\testFiles\verboseOut_20.out >.\testFiles\testOutput_20.out
FindString .\testFiles\*.uss "social" /v /vf .\testFiles\verboseOut_21.out >.\testFiles\testOutput_21.out
FindString .\testFiles\*.txt "social" /v /vf .\testFiles\verboseOut_22.out >.\testFiles\testOutput_22.out
FindString .\testFiles\*.uss "social" /c /v /vf .\testFiles\verboseOut_23.out >.\testFiles\testOutput_23.out
FindString .\testFiles\*.txt "social" /c /v /vf .\testFiles\verboseOut_24.out >.\testFiles\testOutput_24.out
FindString .\testFiles\1_1.uss "\d+-\d+-\d+" /v /vf .\testFiles\verboseOut_25.out /e >.\testFiles\testOutput_25.out
FindString .\testFiles\1_1.txt "\d+-\d+-\d+" /v /vf .\testFiles\verboseOut_26.out /e >.\testFiles\testOutput_26.out
FindString .\testFiles\1_1.uss "\d+-\d+-\d+" /c /v /vf .\testFiles\verboseOut_27.out /e >.\testFiles\testOutput_27.out
FindString .\testFiles\1_1.txt "\d+-\d+-\d+" /c /v /vf .\testFiles\verboseOut_28.out /e >.\testFiles\testOutput_28.out
FindString .\testFiles\*.uss "\d+-\d+-\d+" /v /vf .\testFiles\verboseOut_29.out /e >.\testFiles\testOutput_29.out
FindString .\testFiles\*.txt "\d+-\d+-\d+" /v /vf .\testFiles\verboseOut_30.out /e >.\testFiles\testOutput_30.out
FindString .\testFiles\*.uss "\d+-\d+-\d+" /c /v /vf .\testFiles\verboseOut_31.out /e >.\testFiles\testOutput_31.out
FindString .\testFiles\*.txt "\d+-\d+-\d+" /c /v /vf .\testFiles\verboseOut_32.out /e >.\testFiles\testOutput_32.out

echo Creating expression from file output
FindString .\testFiles\1_1.uss .\testFiles\FindText.lst /el >.\testFiles\testOutput_33.out
FindString .\testFiles\1_1.txt .\testFiles\FindText.lst /el >.\testFiles\testOutput_34.out
FindString .\testFiles\1_1.uss .\testFiles\FindText.lst /c /el >.\testFiles\testOutput_35.out
FindString .\testFiles\1_1.txt .\testFiles\FindText.lst /c /el >.\testFiles\testOutput_36.out
FindString .\testFiles\*.uss .\testFiles\FindText.lst /el >.\testFiles\testOutput_37.out
FindString .\testFiles\*.txt .\testFiles\FindText.lst /el >.\testFiles\testOutput_38.out
FindString .\testFiles\*.uss .\testFiles\FindText.lst /c /el >.\testFiles\testOutput_39.out
FindString .\testFiles\*.txt .\testFiles\FindText.lst /c /el >.\testFiles\testOutput_40.out
FindString .\testFiles\1_1.uss .\testFiles\RegExp.lst /e /el >.\testFiles\testOutput_41.out
FindString .\testFiles\1_1.txt .\testFiles\RegExp.lst /e /el >.\testFiles\testOutput_42.out
FindString .\testFiles\1_1.uss .\testFiles\RegExp.lst /c /e /el >.\testFiles\testOutput_43.out
FindString .\testFiles\1_1.txt .\testFiles\RegExp.lst /c /e /el >.\testFiles\testOutput_44.out
FindString .\testFiles\*.uss .\testFiles\RegExp.lst /e /el >.\testFiles\testOutput_45.out
FindString .\testFiles\*.txt .\testFiles\RegExp.lst /e /el >.\testFiles\testOutput_46.out
FindString .\testFiles\*.uss .\testFiles\RegExp.lst /c /e /el >.\testFiles\testOutput_47.out
FindString .\testFiles\*.txt .\testFiles\RegExp.lst /c /e /el >.\testFiles\testOutput_48.out

echo Creating recursive directory search output
FindString .\testFiles\*.uss "social" /r >.\testFiles\testOutput_49.out
FindString .\testFiles\*.txt "social" /r >.\testFiles\testOutput_50.out
FindString .\testFiles\*.uss "social" /c /r >.\testFiles\testOutput_51.out
FindString .\testFiles\*.txt "social" /c /r >.\testFiles\testOutput_52.out
FindString .\testFiles\*.uss "\d+-\d+-\d+" /r /e >.\testFiles\testOutput_53.out
FindString .\testFiles\*.txt "\d+-\d+-\d+" /r /e >.\testFiles\testOutput_54.out
FindString .\testFiles\*.uss "\d+-\d+-\d+" /c /r /e >.\testFiles\testOutput_55.out
FindString .\testFiles\*.txt "\d+-\d+-\d+" /c /r /e >.\testFiles\testOutput_56.out

echo Creating files from list output
FindString .\testFiles\FileList.lst "social" /fl >.\testFiles\testOutput_57.out
FindString .\testFiles\FileList.lst "social" /c /fl >.\testFiles\testOutput_58.out
FindString .\testFiles\FileList.lst "\d+-\d+-\d+" /fl /e >.\testFiles\testOutput_59.out
FindString .\testFiles\FileList.lst "\d+-\d+-\d+" /c /fl /e >.\testFiles\testOutput_60.out

echo Creating particular page output
FindString .\testFiles\1_1.uss "social" /p 1 >.\testFiles\testOutput_61.out
FindString .\testFiles\1_1.uss "social" /p 1-3 >.\testFiles\testOutput_62.out
FindString .\testFiles\1_1.uss "social" /p 3- >.\testFiles\testOutput_63.out
FindString .\testFiles\1_1.uss "\d+-\d+-\d+" /p 1 /e >.\testFiles\testOutput_64.out
FindString .\testFiles\1_1.uss "\d+-\d+-\d+" /p 1-3 /e >.\testFiles\testOutput_65.out
FindString .\testFiles\1_1.uss "\d+-\d+-\d+" /p 3- /e >.\testFiles\testOutput_66.out

echo Creating individual file total output
FindString .\testFiles\*.uss "social" /r /t >.\testFiles\testOutput_67.out
FindString .\testFiles\*.uss "\d+-\d+-\d+" /r /t /e >.\testFiles\testOutput_68.out

rem Perform comparisons
cls
echo Preparing to perform output comparison
pause
cls
echo comparing test 1
fc .\ExpectedOutput\testOutput_1.out .\testFiles\testOutput_1.out
pause
cls
echo comparing test 2
fc .\ExpectedOutput\testOutput_2.out .\testFiles\testOutput_2.out
pause
cls
echo comparing test 3
fc .\ExpectedOutput\testOutput_3.out .\testFiles\testOutput_3.out
pause
cls
echo comparing test 4
fc .\ExpectedOutput\testOutput_4.out .\testFiles\testOutput_4.out
pause
cls
echo comparing test 5
fc .\ExpectedOutput\testOutput_5.out .\testFiles\testOutput_5.out
pause
cls
echo comparing test 6
fc .\ExpectedOutput\testOutput_6.out .\testFiles\testOutput_6.out
pause
cls
echo comparing test 7
fc .\ExpectedOutput\testOutput_7.out .\testFiles\testOutput_7.out
pause
cls
echo comparing test 8
fc .\ExpectedOutput\testOutput_8.out .\testFiles\testOutput_8.out
pause
cls
echo comparing test 9
fc .\ExpectedOutput\testOutput_9.out .\testFiles\testOutput_9.out
pause
cls
echo comparing test 10
fc .\ExpectedOutput\testOutput_10.out .\testFiles\testOutput_10.out
pause
cls
echo comparing test 11
fc .\ExpectedOutput\testOutput_11.out .\testFiles\testOutput_11.out
pause
cls
echo comparing test 12
fc .\ExpectedOutput\testOutput_12.out .\testFiles\testOutput_12.out
pause
cls
echo comparing test 13
fc .\ExpectedOutput\testOutput_13.out .\testFiles\testOutput_13.out
pause
cls
echo comparing test 14
fc .\ExpectedOutput\testOutput_14.out .\testFiles\testOutput_14.out
pause
cls
echo comparing test 15
fc .\ExpectedOutput\testOutput_15.out .\testFiles\testOutput_15.out
pause
cls
echo comparing test 16
fc .\ExpectedOutput\testOutput_16.out .\testFiles\testOutput_16.out
pause
cls
echo comparing test 17
fc .\ExpectedOutput\testOutput_17.out .\testFiles\testOutput_17.out
pause
cls
echo comparing test 18
fc .\ExpectedOutput\testOutput_18.out .\testFiles\testOutput_18.out
pause
cls
echo comparing test 19
fc .\ExpectedOutput\testOutput_19.out .\testFiles\testOutput_19.out
pause
cls
echo comparing test 20
fc .\ExpectedOutput\testOutput_20.out .\testFiles\testOutput_20.out
pause
cls
echo comparing test 21
fc .\ExpectedOutput\testOutput_21.out .\testFiles\testOutput_21.out
pause
cls
echo comparing test 22
fc .\ExpectedOutput\testOutput_22.out .\testFiles\testOutput_22.out
pause
cls
echo comparing test 23
fc .\ExpectedOutput\testOutput_23.out .\testFiles\testOutput_23.out
pause
cls
echo comparing test 24
fc .\ExpectedOutput\testOutput_24.out .\testFiles\testOutput_24.out
pause
cls
echo comparing test 25
fc .\ExpectedOutput\testOutput_25.out .\testFiles\testOutput_25.out
pause
cls
echo comparing test 26
fc .\ExpectedOutput\testOutput_26.out .\testFiles\testOutput_26.out
pause
cls
echo comparing test 27
fc .\ExpectedOutput\testOutput_27.out .\testFiles\testOutput_27.out
pause
cls
echo comparing test 28
fc .\ExpectedOutput\testOutput_28.out .\testFiles\testOutput_28.out
pause
cls
echo comparing test 29
fc .\ExpectedOutput\testOutput_29.out .\testFiles\testOutput_29.out
pause
cls
echo comparing test 30
fc .\ExpectedOutput\testOutput_30.out .\testFiles\testOutput_30.out
pause
cls
echo comparing test 31
fc .\ExpectedOutput\testOutput_31.out .\testFiles\testOutput_31.out
pause
cls
echo comparing test 32
fc .\ExpectedOutput\testOutput_32.out .\testFiles\testOutput_32.out
pause
cls
echo comparing test 33
fc .\ExpectedOutput\testOutput_33.out .\testFiles\testOutput_33.out
pause
cls
echo comparing test 34
fc .\ExpectedOutput\testOutput_34.out .\testFiles\testOutput_34.out
pause
cls
echo comparing test 35
fc .\ExpectedOutput\testOutput_35.out .\testFiles\testOutput_35.out
pause
cls
echo comparing test 36
fc .\ExpectedOutput\testOutput_36.out .\testFiles\testOutput_36.out
pause
cls
echo comparing test 37
fc .\ExpectedOutput\testOutput_37.out .\testFiles\testOutput_37.out
pause
cls
echo comparing test 38
fc .\ExpectedOutput\testOutput_38.out .\testFiles\testOutput_38.out
pause
cls
echo comparing test 39
fc .\ExpectedOutput\testOutput_39.out .\testFiles\testOutput_39.out
pause
cls
echo comparing test 40
fc .\ExpectedOutput\testOutput_40.out .\testFiles\testOutput_40.out
pause
cls
echo comparing test 41
fc .\ExpectedOutput\testOutput_41.out .\testFiles\testOutput_41.out
pause
cls
echo comparing test 42
fc .\ExpectedOutput\testOutput_42.out .\testFiles\testOutput_42.out
pause
cls
echo comparing test 43
fc .\ExpectedOutput\testOutput_43.out .\testFiles\testOutput_43.out
pause
cls
echo comparing test 44
fc .\ExpectedOutput\testOutput_44.out .\testFiles\testOutput_44.out
pause
cls
echo comparing test 45
fc .\ExpectedOutput\testOutput_45.out .\testFiles\testOutput_45.out
pause
cls
echo comparing test 46
fc .\ExpectedOutput\testOutput_46.out .\testFiles\testOutput_46.out
pause
cls
echo comparing test 47
fc .\ExpectedOutput\testOutput_47.out .\testFiles\testOutput_47.out
pause
cls
echo comparing test 48
fc .\ExpectedOutput\testOutput_48.out .\testFiles\testOutput_48.out
pause
cls
echo comparing test 49
fc .\ExpectedOutput\testOutput_49.out .\testFiles\testOutput_49.out
pause
cls
echo comparing test 50
fc .\ExpectedOutput\testOutput_50.out .\testFiles\testOutput_50.out
pause
cls
echo comparing test 51
fc .\ExpectedOutput\testOutput_51.out .\testFiles\testOutput_51.out
pause
cls
echo comparing test 52
fc .\ExpectedOutput\testOutput_52.out .\testFiles\testOutput_52.out
pause
cls
echo comparing test 53
fc .\ExpectedOutput\testOutput_53.out .\testFiles\testOutput_53.out
pause
cls
echo comparing test 54
fc .\ExpectedOutput\testOutput_54.out .\testFiles\testOutput_54.out
pause
cls
echo comparing test 55
fc .\ExpectedOutput\testOutput_55.out .\testFiles\testOutput_55.out
pause
cls
echo comparing test 56
fc .\ExpectedOutput\testOutput_56.out .\testFiles\testOutput_56.out
pause
cls
echo comparing test 57
fc .\ExpectedOutput\testOutput_57.out .\testFiles\testOutput_57.out
pause
cls
echo comparing test 58
fc .\ExpectedOutput\testOutput_58.out .\testFiles\testOutput_58.out
pause
cls
echo comparing test 59
fc .\ExpectedOutput\testOutput_59.out .\testFiles\testOutput_59.out
pause
cls
echo comparing test 60
fc .\ExpectedOutput\testOutput_60.out .\testFiles\testOutput_60.out
pause
cls
echo comparing test 61
fc .\ExpectedOutput\testOutput_61.out .\testFiles\testOutput_61.out
pause
cls
echo comparing test 62
fc .\ExpectedOutput\testOutput_62.out .\testFiles\testOutput_62.out
pause
cls
echo comparing test 63
fc .\ExpectedOutput\testOutput_63.out .\testFiles\testOutput_63.out
pause
cls
echo comparing test 64
fc .\ExpectedOutput\testOutput_64.out .\testFiles\testOutput_64.out
pause
cls
echo comparing test 65
fc .\ExpectedOutput\testOutput_65.out .\testFiles\testOutput_65.out
pause
cls
echo comparing test 66
fc .\ExpectedOutput\testOutput_66.out .\testFiles\testOutput_66.out
pause
cls
echo comparing test 67
fc .\ExpectedOutput\testOutput_67.out .\testFiles\testOutput_67.out
pause
cls
echo comparing test 68
fc .\ExpectedOutput\testOutput_68.out .\testFiles\testOutput_68.out
pause
cls

rem Compare the verbose output
echo Preparing to compare verbose output
pause
cls
echo comparing verbose output 1 (17)
fc .\ExpectedOutput\verboseOut_17.out .\testFiles\verboseOut_17.out
pause
cls
echo comparing verbose output 2 (18)
fc .\ExpectedOutput\verboseOut_18.out .\testFiles\verboseOut_18.out
pause
cls
echo comparing verbose output 3 (19)
fc .\ExpectedOutput\verboseOut_19.out .\testFiles\verboseOut_19.out
pause
cls
echo comparing verbose output 4 (20)
fc .\ExpectedOutput\verboseOut_20.out .\testFiles\verboseOut_20.out
pause
cls
echo comparing verbose output 5 (21)
fc .\ExpectedOutput\verboseOut_21.out .\testFiles\verboseOut_21.out
pause
cls
echo comparing verbose output 6 (22)
fc .\ExpectedOutput\verboseOut_22.out .\testFiles\verboseOut_22.out
pause
cls
echo comparing verbose output 7 (23)
fc .\ExpectedOutput\verboseOut_23.out .\testFiles\verboseOut_23.out
pause
cls
echo comparing verbose output 8 (24)
fc .\ExpectedOutput\verboseOut_24.out .\testFiles\verboseOut_24.out
pause
cls
echo comparing verbose output 9 (25)
fc .\ExpectedOutput\verboseOut_25.out .\testFiles\verboseOut_25.out
pause
cls
echo comparing verbose output 10 (26)
fc .\ExpectedOutput\verboseOut_26.out .\testFiles\verboseOut_26.out
pause
cls
echo comparing verbose output 11 (27)
fc .\ExpectedOutput\verboseOut_27.out .\testFiles\verboseOut_27.out
pause
cls
echo comparing verbose output 12 (28)
fc .\ExpectedOutput\verboseOut_28.out .\testFiles\verboseOut_28.out
pause
cls
echo comparing verbose output 13 (29)
fc .\ExpectedOutput\verboseOut_29.out .\testFiles\verboseOut_29.out
pause
cls
echo comparing verbose output 14 (30)
fc .\ExpectedOutput\verboseOut_30.out .\testFiles\verboseOut_30.out
pause
cls
echo comparing verbose output 15 (31)
fc .\ExpectedOutput\verboseOut_31.out .\testFiles\verboseOut_31.out
pause
cls
echo comparing verbose output 16 (32)
fc .\ExpectedOutput\verboseOut_32.out .\testFiles\verboseOut_32.out
pause
cls

rem Perform error condition testing
pause
echo Preparing to test error conditions
pause
cls
echo Confirm usage is displayed
FindString
pause
echo Confirm invalid command line argument error and usage displayed
FindString .\testFiles\1_1.uss "social" /g
pause
echo Confirm /ef must have a file error and usage displayed
FindString .\testFiles\1_1.uss "social" /ef
pause
echo Confirm file does not exist exception
FindString .\testFiles\ThisFileDoesNotExist.uss "social"
pause
echo Confirm file testException.uex exists with current timestamp
FindString .\testFiles\BadFileList.lst "social" /fl /ef .\testException.uex
pause
echo Confirm exception file must end in .uex exception displayed
FindString .\testFiles\1_1.uss "social" /ef .\test.exp
pause
echo Confirm exception file must end in .uex exception displayed
FindString .\testFiles\1_1.uss "social" /ef /c
pause
echo Confirm /p must have a page specification error and usage displayed
FindString .\testFiles\1_1.uss "social" /p
pause
echo Confirm /v /t together error and usage displayed
FindString .\testFiles\1_1.uss "social" /v /t
pause
echo Confirm /v /t together error and usage displayed
FindString .\testFiles\1_1.uss "social" /t /v
pause

echo Testing complete!
pause
cls

rem Cleanup the copied files and output files
echo Cleaning up
call clean.bat
pause
