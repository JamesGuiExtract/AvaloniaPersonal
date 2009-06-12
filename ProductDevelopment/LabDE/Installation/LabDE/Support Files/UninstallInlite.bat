
@echo off
for %%I in ("C:\Program Files\Common Files\ClearImage\*.dll") do regsvr32 /s /u "%%~fI"

rmdir /s/q "C:\Program Files\Common Files\ClearImage"

rmdir /s/q "C:\Program Files\Inlite"
