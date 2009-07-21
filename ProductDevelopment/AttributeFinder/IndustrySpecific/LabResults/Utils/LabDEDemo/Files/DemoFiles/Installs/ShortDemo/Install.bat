@echo off

del ..\CurrentDemo\TIF\*.* /q /s
del ..\CurrentDemo\VOA\*.* /q /s

copy ..\LongDemo\TIF\418.* ..\CurrentDemo\TIF
copy ..\LongDemo\VOA\418.* ..\CurrentDemo\VOA

copy ..\LongDemo\TIF\498.* ..\CurrentDemo\TIF
copy ..\LongDemo\VOA\498.* ..\CurrentDemo\VOA

copy ..\LongDemo\TIF\221.* ..\CurrentDemo\TIF
copy ..\LongDemo\VOA\221.* ..\CurrentDemo\VOA

copy ..\LongDemo\TIF\352.* ..\CurrentDemo\TIF
copy ..\LongDemo\VOA\352.* ..\CurrentDemo\VOA

PAUSE
