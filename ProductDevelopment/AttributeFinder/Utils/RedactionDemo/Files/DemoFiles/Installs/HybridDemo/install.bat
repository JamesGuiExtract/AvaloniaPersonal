@echo off
C:
CD C:\Demo_IDShield\DemoFiles\Installs\HybridDemo

del ..\..\..\Input\*.* /s /q
del ..\..\..\FPS\*.* /s /q

copy FPS\*.* ..\..\..\FPS
copy Input\*.* ..\..\..\Input

Pause
