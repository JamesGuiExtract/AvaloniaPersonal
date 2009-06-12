@echo off

del ..\..\..\Input\*.* /s /q
del ..\..\..\FPS\*.* /s /q

copy FPS\*.* ..\..\..\FPS
copy Input\*.* ..\..\..\Input

Pause
