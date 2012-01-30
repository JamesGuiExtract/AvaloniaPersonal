@echo off
C:
CD C:\Demo_IDShield\DemoFiles\Installs\VerifyAllInterestingDocs

del ..\..\..\Input\*.* /s /q
del ..\..\..\FPS\*.* /s /q

copy FPS\*.* ..\..\..\FPS
cd Input
call Install.bat

Pause
