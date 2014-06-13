mkdir d:\h
net use h: \\127.0.0.1\d$\h
path "%programfiles%\Extract Systems\CommonComponents";%PATH%
call clean.bat

:: exercise the RSD files that provide output to testResults.txt
runrules 1Test_DirOfFileOfSourceDocName.rsd "Test  Image.tif"
runrules 2Test_FileNoExtOf.rsd "Test  Image.tif"
runrules 3Test_TrimWS.rsd "Test  Image.tif"
runrules 4Test_PadOffsetExtOf.rsd "Test  Image.tif"
runrules 5Test_RSDFileDir.rsd "Test  Image.tif"
runrules 6Test_XMLOutputFile.rsd "Test  Image.tif"
runrules 7Test_VOAOutputFile.rsd "Test  Image.tif"
runrules 8Test_DocTypeTag.rsd "Test  Image.tif"
runrules 9Test_DriveOfTag.rsd "Test  Image.tif"
runrules 10Test_RuleExecutionID.rsd "Test  Image.tif"
runrules 11Test_DirNoDriveOfTag.rsd "Test  Image.tif"
runrules 12Test_ReplaceTag.rsd "Test  Image.tif"
runrules 13Test_InsertBeforeExt.rsd "Test  Image.tif"
runrules 14Test_Env.rsd "Test  Image.tif"
runrules 19Test_ChangeExt.rsd "Test  Image.tif"
runrules 22Test_LeftMidRight.rsd "Test  Image.tif"

:: the following tests will be run, but no corresponding output will be in testResults.txt
runrules 15Test_FullUserName.rsd "Test  Image.tif"
runrules 16Test_Now.rsd "Test  Image.tif"
runrules 17Test_RandomAlphaNumeric.rsd "Test  Image.tif"
runrules 18Test_UserName.rsd "Test  Image.tif"
runrules 20Test_ProcessId.rsd "Test  Image.tif"
runrules 21Test_ThreadId.rsd "Test  Image.tif"
runrules 23Test_RandomEntryFromListFile.rsd "Test  Image.tif"
runrules 24Test_RandomEntryFromList.rsd "Test  Image.tif"
pause

:: Write results of test to "testResults.txt"
:: Check that output files exist.
echo off
dir /B "01_Test  Image.tif.xml" >> testResults.txt
if errorlevel 1 echo "01_Test  Image.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "02_Test  Image.xml" >> testResults.txt
if errorlevel 1 echo "02_Test  Image.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "03_Test Image.xml" >> testResults.txt
if errorlevel 1 echo "03_Test Image.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "04_Test01.tif.xml" >> testResults.txt
if errorlevel 1 echo "04_Test01.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "05_TestRSDFileDirTag.xml" >> testResults.txt
if errorlevel 1 echo "05_TestRSDFileDirTag.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "Test  Image.tif.xml_06.test" >> testResults.txt
if errorlevel 1 echo "Test  Image.tif.xml_06.test not created" >> testResults.txt
echo. >> testResults.txt

dir /B "Test  Image.tif.voa_07.test" >> testResults.txt
if errorlevel 1 echo "Test  Image.tif.voa_07.test not created" >> testResults.txt
echo. >> testResults.txt

dir /B "08_TestWarranty Deed.xml" >> testResults.txt
if errorlevel 1 echo "08_TestWarranty Deed.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "H:\09_TestDriveOfTag.xml" >> testResults.txt
if errorlevel 1 echo "H:\09_TestDriveOfTag.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "10_TestRuleExecutionID*.xml" >> testResults.txt
if errorlevel 1 echo "10_TestRuleExecutionID??.xml not created" >> testResults.txt
echo. >> testResults.txt

rem dir /B /S "I:\users\wayne_lenius\Public\Write\ABD\Test  Image.tif.xml" >> testResults.txt
rem if errorlevel 1 echo "I:\users\wayne_lenius\Public\Write\ABD\Test  Image.tif.xml not created" >> testResults.txt
rem echo. >> testResults.txt

rem echo "Check for Test  Image.tif.xml under a replicated directory tree in drive H:\"  >> testResults.txt
rem echo. >> testResults.txt

dir /B /S "h:\Engineering\ReusableComponents\BaseUtils\InteractiveTest\11_Test  Image.tif.xml" >> testResults.txt
if errorlevel 1 echo "h:\Engineering\ReusableComponents\BaseUtils\InteractiveTest\11_Test  Image.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B /S "h:\11B_Test  Image.tif.xml" >> testResults.txt
if errorlevel 1 echo "h:\11B_Test  Image.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B /S "H:\common\11C_Test  Image.tif.xml" >> testResults.txt
if errorlevel 1 echo "H:\common\11C_Test  Image.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B /S "H:\11D_Test  Image.tif.xml" >> testResults.txt
if errorlevel 1 echo "H:\11D_Test  Image.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "12_Tst  Imag.tif.xml" >> testResults.txt
if errorlevel 1 echo "12_Tst  Imag.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "12B_TestFile.tif.xml" >> testResults.txt
if errorlevel 1 echo "12B_TestFile.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "12C_Test  Image.tif.xml" >> testResults.txt
if errorlevel 1 echo "12C_Test  Image.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "Test  Image.INSERT_13.tif.xml" >> testResults.txt
if errorlevel 1 echo "Test  Image.INSERT_13.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "Test  Image.tif.INSERT_13B.xml" >> testResults.txt
if errorlevel 1 echo "Test  Image.tif.INSERT_13B.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "14_%userdomain%.xml" >> testResults.txt
if errorlevel 1 echo "14_%userdomain%.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "19_Test  Image.ABC.xml" >> testResults.txt
if errorlevel 1 echo "19_Test  Image.ABC.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "22L_Test.xml" >> testResults.txt
if errorlevel 1 echo "22L_Test.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "22M_t  Im.xml" >> testResults.txt
if errorlevel 1 echo "22M_t  Im.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "22R_mage.xml" >> testResults.txt
if errorlevel 1 echo "22R_mage.xml not created" >> testResults.txt
echo. >> testResults.txt

echo "Test complete. See testResults.txt for output."
pause

