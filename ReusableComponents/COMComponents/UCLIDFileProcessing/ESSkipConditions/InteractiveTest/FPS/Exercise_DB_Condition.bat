:: Add CommonComponents to local PATH
SET PROGRAM_ROOT=%ProgramFiles(x86)%

IF NOT "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	SET PROGRAM_ROOT=%ProgramFiles%
)
path "%PROGRAM_ROOT%\Extract Systems\CommonComponents";%PATH%
:: Remove any existing OUTPUT files
call clean.bat

:::::::::::
:: Exercise the FPS files that use DB_Condition_Test.SDF
:::::::::::

:: Exercise Contains Rows
runfpsfile "01_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "02_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "03_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing

:: Exercise Contains Zero Rows With Any, All, None
runfpsfile "04a_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "04b_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "05a_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "05b_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "06a_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "06b_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing

:: Exercise Contains Exactly One Row With Any, All, None
:: 08c, 08d, 08e exercise case-sensitive and fuzzy settings
runfpsfile "07a_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "07b_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "07c_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing /ef "D:\DBCondition.uex"
runfpsfile "08a_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "08b_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "08c_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "08d_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "08e_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "09a_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "09b_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing

:: Exercise Query for Exactly One, Zero, At Least One Row
runfpsfile "10a_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "10b_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "11a_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "11b_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "12a_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "12b_SDF.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing

:::::::::::
:: Exercise the FPS files that use FAM database
:::::::::::

:: Exercise Contains Rows
runfpsfile "01_FAM.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "02a_FAM.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "02b_FAM.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "03a_FAM.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "03b_FAM.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing

:: Exercise Contains At Least One Row With Any, All, None
:: 05c, 05d exercise case-sensitive and fuzzy settings
runfpsfile "04a_FAM.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "04b_FAM.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "05a_FAM.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "05b_FAM.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "05c_FAM.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "05d_FAM.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "06a_FAM.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing
runfpsfile "06b_FAM.fps" "..\Images\TestImageB.tif" /queue /process /forceProcessing

pause
