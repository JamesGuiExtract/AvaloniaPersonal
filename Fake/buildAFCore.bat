call fake run build.fsx -t AFCoreTestBuild.Build
call fake run buildAFCoreTest.fsx -t All.Build -p 8
pause
