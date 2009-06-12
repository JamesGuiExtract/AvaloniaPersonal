@ECHO OFF
RunRules.exe %1 %2 %3
IF ERRORLEVEL 1 GOTO exception_caught
GOTO no_exception_caught

:exception_caught
ECHO Exception caught!
GOTO quit

:no_exception_caught
ECHO Command successful!
GOTO quit

:quit
