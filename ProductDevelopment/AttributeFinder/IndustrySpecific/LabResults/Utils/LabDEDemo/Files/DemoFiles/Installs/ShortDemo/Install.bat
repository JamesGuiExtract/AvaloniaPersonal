@echo off

del ..\CurrentDemo\TIF\*.* /q /s
del ..\CurrentDemo\VOA\*.* /q /s

copy ..\LongDemo\TIF\E418.* ..\CurrentDemo\TIF
copy ..\LongDemo\VOA\E418.* ..\CurrentDemo\VOA

copy ..\LongDemo\TIF\C498.* ..\CurrentDemo\TIF
copy ..\LongDemo\VOA\C498.* ..\CurrentDemo\VOA

copy ..\LongDemo\TIF\B221.* ..\CurrentDemo\TIF
copy ..\LongDemo\VOA\B221.* ..\CurrentDemo\VOA

copy ..\LongDemo\TIF\Z352.* ..\CurrentDemo\TIF
copy ..\LongDemo\VOA\Z352.* ..\CurrentDemo\VOA

PAUSE
