:: KillAllOCRInstances.bat - kills all running SSOCR2 processes
::
@echo off

if [%1]==[/q] goto killOCR

if not [%1]==[] goto description

setlocal

goto promptToKill


:description

  if not [%1]==[/?] echo Invalid parameter: %1 & echo.

  echo Kills all running SSOCR2 processes.
  echo.
  echo %0 [/q]
  echo.
  echo /q      Quiet mode. Kills SSOCR2 without prompting.

  goto end


:promptToKill

  set /p killOCR=Are you sure you want to kill all running SSOCR2 instances (Y/N)?

  if /i [%killOCR%]==[N] goto end
  if /i [%killOCR%]==[NO] goto end

  if /i [%killOCR%]==[Y] goto killOCR
  if /i [%killOCR%]==[YES] goto killOCR

  goto promptToKill


:killOCR

  taskkill /F /IM ssocr2*


:end
