@ECHO OFF

sc config NLS start=delayed-auto

:: Set a RightsToAdd variable to the rights to add
@SET rightsToAdd=(A;;CCLCRPWPLO;;;BU)

:: Set oldrights to the current rights for the Nuance service
for /F "tokens=*" %%R in ('sc sdshow NLS') do set oldrights=%%R

:: Check if the rights you want to add are not already there
@Echo. %oldrights% | findstr /C:%rightsToAdd% 1>nul
:: Nothing to do if the rights are already there so goto done
if NOT errorlevel 1 GOTO DONE

:: Check if the S: section is in the oldrights
@ECHO. %oldrights% | findstr /C:S: 1>nul

:: If the S: section was found goto PlaceBeforeS
if NOT errorlevel 1 GOTO PlaceBeforeS

:: No S: Section so just append to oldrights and set new rights
sc sdset NLS %oldrights%%rightsToAdd%

goto DONE

:PlaceBeforeS

:: The S: section was found so put the new rights before the S:
sc sdset NLS %oldrights:S:=(A;;CCLCRPWPLO;;;BU)S:%

:DONE