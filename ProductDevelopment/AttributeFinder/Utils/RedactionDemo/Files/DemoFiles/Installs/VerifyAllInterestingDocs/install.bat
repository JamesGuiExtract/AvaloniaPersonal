@echo off

C:
CD %~dp0

del ..\..\..\Input\*.* /s /q
del ..\..\..\FPS\*.* /s /q

md ..\..\..\FPS
copy FPS\*.* ..\..\..\FPS\
cd Input
call Install.bat

Pause
