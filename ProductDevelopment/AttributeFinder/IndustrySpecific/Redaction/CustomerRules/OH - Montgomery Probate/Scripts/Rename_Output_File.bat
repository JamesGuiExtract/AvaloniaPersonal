echo off
REM The TXT file used to store information about files that 
REM have gone through the redaction and verification process
REM is read by the document management system to determine 
REM which files contain redactions.  
REM 
REM The VBS script file that populates the TXT file has been 
REM modified to write its information to Redacted_Output_TEMP.txt.
REM This batch file will rename the file to Redacted_Output.txt 
REM as expected by the document management system.
REM 
:: Confirm that output file does not exist
if exist ".\Redacted_Output.txt" (
   echo "Redacted_Output.txt file already exists and will not be overwritten!"
   goto end
)

:: Rename the temporary file
echo on
move Redacted_Output_TEMP.txt Redacted_Output.txt

:end
pause
