call fake run build.fsx -t UtilsBuild.Build
call fake run buildUtils.fsx -t All.Build -p 8
pause
