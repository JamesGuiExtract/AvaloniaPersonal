mkdir d:\h
net use h: \\127.0.0.1\d$\h
::SET FLEX_INDEX_TEST="FLEX Index test"
path "%programfiles(x86)%\Extract Systems\CommonComponents";%PATH%
feedback.exe
call clean.bat
runrules 1Test_DirOfFileOfSourceDocName.rsd "Test  Image.tif"
runrules 2Test_FileNoExtOf.rsd "Test  Image.tif"
runrules 3Test_TrimWS.rsd "Test  Image.tif"
runrules 4Test_PadOffsetExtOf.rsd "Test  Image.tif"
runrules 5Test_RSDFileDir.rsd "Test  Image.tif"
runrules 6Test_XMLOutputFile.rsd "Test  Image.tif"
runrules 7Test_VOAOutputFile.rsd "Test  Image.tif"
runrules 8Test_DocTypeTag.rsd "Test  Image.tif"
runrules 9Test_DriveOfTag.rsd "Test  Image.tif"
echo Moving \TextDriveOfTag.xml to current working directory...
sleep 1
move \TestDriveOfTag.xml .\
runrules 10Test_RuleExecutionID.rsd "Test  Image.tif"
runrules 11Test_DirNoDriveOfTag.rsd "Test  Image.tif"
runrules 12Test_ReplaceTag.rsd "Test  Image.tif"
runrules 13Test_InsertBeforeExt.rsd "Test  Image.tif"
runrules 14Test_Env.rsd "Test  Image.tif"

:: the following tests will be run, but no corresponding output will be in testResults.txt
runrules 15Test_FullUserName.rsd "Test  Image.tif"
runrules 16Test_Now.rsd "Test  Image.tif"
runrules 17Test_RandomAlphaNumeric.rsd "Test  Image.tif"
runrules 18Test_UserName.rsd "Test  Image.tif"
pause

:: Write results of test to "testResults.txt"
:: Check that output files existence.
echo off
dir /B "Test  Image.tif.xml" >> testResults.txt
if errorlevel 1 echo "Test  Image.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "Test  Image.xml" >> testResults.txt
if errorlevel 1 echo "Test  Image.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "Test Image.xml" >> testResults.txt
if errorlevel 1 echo "Test Image.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "Test01.tif.xml" >> testResults.txt
if errorlevel 1 echo "Test01.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "TestRSDFileDirTag.xml" >> testResults.txt
if errorlevel 1 echo "TestRSDFileDirTag.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "Test  Image.tif.xml.test" >> testResults.txt
if errorlevel 1 echo "Test  Image.tif.xml.test not created" >> testResults.txt
echo. >> testResults.txt

dir /B "Test  Image.tif.voa.test" >> testResults.txt
if errorlevel 1 echo "Test  Image.tif.voa.test not created" >> testResults.txt
echo. >> testResults.txt

dir /B "TestWarranty Deed.xml" >> testResults.txt
if errorlevel 1 echo "TestWarranty Deed.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "TestRuleExecutionID*.xml" >> testResults.txt
if errorlevel 1 echo "TestRuleExecutionID??.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "TestDriveOfTag.xml" >> testResults.txt
if errorlevel 1 echo "TestDriveOfTag.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B /S "I:\users\wayne_lenius\Public\Write\ABD\Test  Image.tif.xml" >> testResults.txt
if errorlevel 1 echo "I:\users\wayne_lenius\Public\Write\ABD\Test  Image.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

echo "Check for Test  Image.tif.xml under a replicated directory tree in drive H:\"  >> testResults.txt
echo. >> testResults.txt

dir /B /S "h:\Test  Image.tif.xml" >> testResults.txt
if errorlevel 1 echo "h:\Test  Image.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B /S "\\fnp2\internal\Common\Testing\product testing\TestTextFunctionExpander\Test  Image.tif.xml" >> testResults.txt
if errorlevel 1 echo "\\fnp2\internal\Common\Testing\product testing\TestTextFunctionExpander\Test  Image.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B /S "\\fnp2\internal\Common\Testing\product testing\TestTextFunctionExpander\ABC\Test  Image.tif.xml" >> testResults.txt
if errorlevel 1 echo "\\fnp2\internal\Common\Testing\product testing\TestTextFunctionExpander\ABC\Test  Image.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "%userdomain%.xml" >> testResults.txt
if errorlevel 1 echo "%userdomain%.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "Test  Image.INSERT1.tif.xml" >> testResults.txt
if errorlevel 1 echo "Test  Image.INSERT1.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "Test  Image.tif.INSERT2.xml" >> testResults.txt
if errorlevel 1 echo "Test  Image.tif.INSERT2.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "Tst  Imag.tif.xml" >> testResults.txt
if errorlevel 1 echo "Tst  Imag.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "TestFile.tif.xml" >> testResults.txt
if errorlevel 1 echo "TestFile.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "TestFile.tif.xml" >> testResults.txt
if errorlevel 1 echo "TestFile.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

dir /B "Test  Image.tif.xml" >> testResults.txt
if errorlevel 1 echo "Test  Image.tif.xml not created" >> testResults.txt
echo. >> testResults.txt

echo "Test complete. See testResults.txt for output."
pause

