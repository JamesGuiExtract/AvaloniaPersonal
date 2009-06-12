echo off
REM Build VBS file for delivery to customer using various
REM Extract Systems utility scripts and a customer-specific 
REM script file that may use one or more utility subroutines
REM 
REM Usage:
REM    Build_Final_Script customer-specific.vbs customer-master.vbs
REM 
REM where:
REM    customer-specific.vbs = collection of statements and functions specific to this delivery
REM    
REM    customer-master.vbs = final script file for delivery as a combination of other files
REM 
REM The customer-master.vbs file is built from the following order of files:
REM    ScriptHeader.vbs
REM    customer-specific.vbs
REM    CustomerUtils.vbs
REM    Debug_And_ErrorHandling.vbs
REM 
echo on
copy ".\..\..\..\..\..\..\..\ReusableComponents\Scripts\VBScript\ScriptHeader.vbs" + "%1" + "CustomerUtils.vbs" +  ".\..\..\..\..\..\..\..\ReusableComponents\Scripts\VBScript\Debug_And_ErrorHandling.vbs" "%2" /B /Y
pause
