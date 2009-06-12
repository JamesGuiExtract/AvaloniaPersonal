@echo off

del ..\..\..\Input\*.* /s /q
del ..\..\..\FPS\*.* /s /q

copy FPS\*.* ..\..\..\FPS
cd Input
call Install.bat

Pause
