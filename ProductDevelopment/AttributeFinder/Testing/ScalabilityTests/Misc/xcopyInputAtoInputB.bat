@echo off
ECHO This batch file will copy the directory tree 
ECHO \\jake\FlexIndexTesting\ScalabilityTests\Test1\Input\A
ECHO 		to
ECHO \\jake\FlexIndexTesting\ScalabilityTests\Test1\Input\B
pause

md \\jake\FlexIndexTesting\ScalabilityTests\Test1\Input\B
xcopy \\jake\FlexIndexTesting\ScalabilityTests\Test1\Input\A \\jake\FlexIndexTesting\ScalabilityTests\Test1\Input\B
