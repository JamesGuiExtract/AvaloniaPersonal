@echo off

del ..\CurrentDemo\TIF\*.* /q /s
del ..\CurrentDemo\VOA\*.* /q /s

copy TIF\*.* ..\CurrentDemo\TIF
copy VOA\*.* ..\CurrentDemo\VOA

PAUSE
