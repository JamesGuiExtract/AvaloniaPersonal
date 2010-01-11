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
pause