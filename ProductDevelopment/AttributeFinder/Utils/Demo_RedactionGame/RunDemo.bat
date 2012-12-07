@echo off

C:
CD C:\Demo_RedactionGame

IF EXIST "%ProgramFiles(x86)%\Extract Systems\CommonComponents" GOTO INIT64BITPATH
IF EXIST "%ProgramFiles%\Extract Systems\CommonComponents" GOTO INIT32BITPATH
GOTO PATH_ERROR

:PATH_ERROR
ECHO ERROR! Extract Systems Common Components folder not found!
GOTO DONE

:INIT64BITPATH
SET CCPATH="%ProgramFiles(x86)%\Extract Systems\CommonComponents"
GOTO STEP2

:INIT32BITPATH
SET CCPATH="%ProgramFiles%\Extract Systems\CommonComponents"
GOTO STEP2

:STEP2

REM Prompt the operator to enter their details
CLS
ECHO Prompting for contestant information....
Misc\DataEntryPrompt.exe

GOTO DONE

:DONE
