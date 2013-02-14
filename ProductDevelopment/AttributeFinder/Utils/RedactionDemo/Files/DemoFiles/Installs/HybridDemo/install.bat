@echo off

C:
CD %~dp0

del ..\..\..\Input\*.* /s /q
del ..\..\..\FPS\*.* /s /q

copy FPS\*.* ..\..\..\FPS
copy Input\*.* ..\..\..\Input

Pause
