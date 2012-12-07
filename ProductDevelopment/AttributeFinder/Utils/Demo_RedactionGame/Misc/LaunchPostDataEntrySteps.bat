@echo off

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

REM %1=FirstName
REM %2=LastName
REM %3=Company
REM %4=Title
REM %5=Phone
REM %6=Email
REM %7=Comments

SET FinalReportName=C:\Demo_RedactionGame\Misc\RedactionGameReportFinal3333.txt
SET HeaderFileName=C:\Demo_RedactionGame\Misc\ReportHeader1111.txt
SET FooterFileName=C:\Demo_RedactionGame\Misc\ReportFooter2222.txt

REM Launch the image verification

ECHO. > %HeaderFileName%
ECHO Welcome to the ID Shield Redaction Game! >> %HeaderFileName%
ECHO. >> %HeaderFileName%
ECHO ------------------------------ >> %HeaderFileName%
ECHO Contestant Details >> %HeaderFileName%
ECHO ------------------------------ >> %HeaderFileName%
ECHO First name : %1 >> %HeaderFileName%
ECHO Last name  : %2 >> %HeaderFileName%
ECHO Company    : %3 >> %HeaderFileName%
ECHO Title      : %4 >> %HeaderFileName%
ECHO Phone      : %5 >> %HeaderFileName%
ECHO Email      : %6 >> %HeaderFileName%
ECHO Comments   : %7 >> %HeaderFileName%
ECHO. >> %HeaderFileName%

ECHO. > %FooterFileName%
ECHO ------------------------------ >> %FooterFileName%
ECHO Productivity results >> %FooterFileName%
ECHO ------------------------------ >> %FooterFileName%
ECHO Typical speed for manual human image verification :   500 pages/hr >> %FooterFileName%
ECHO Typical speed for automatic redaction by ID Shield: 4,000 pages/hr >> %FooterFileName%
ECHO. >> %FooterFileName%
ECHO You started redacting images at: >> %FooterFileName%
TIME /T >> %FooterFileName%
%CCPATH%\ProcessFiles.exe C:\Demo_RedactionGame\FPS\Verify.fps /s /fc
GOTO DONE

:DONE
