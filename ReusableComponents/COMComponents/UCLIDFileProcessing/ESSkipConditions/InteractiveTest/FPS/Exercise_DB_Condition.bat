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
CALL :RunWithDB 01_SDF_ZeroRows_withDB 01_SDF.fps
CALL :RunIgnoreDB 01_SDF_ZeroRows_ignoreDB 01_SDF.fps
CALL :RunWithDB 02_SDF_ExactlyOneRow_withDB 02_SDF.fps
CALL :RunIgnoreDB 02_SDF_ExactlyOneRow_ignoreDB 02_SDF.fps
CALL :RunWithDB 03_SDF_AtLeastOneRow_withDB 03_SDF.fps
CALL :RunIgnoreDB 03_SDF_AtLeastOneRow_ignoreDB 03_SDF.fps

:: Exercise Contains Zero Rows With Any, All, None
CALL :RunWithDB 04a_SDF_ZeroRowsWithAny_withDB 04a_SDF.fps
CALL :RunIgnoreDB 04a_SDF_ZeroRowsWithAny_ignoreDB 04a_SDF.fps
CALL :RunWithDB 04b_SDF_ZeroRowsWithAny_withDB 04b_SDF.fps
CALL :RunIgnoreDB 04b_SDF_ZeroRowsWithAny_ignoreDB 04b_SDF.fps
CALL :RunWithDB 05a_SDF_ZeroRowsWithAll_withDB 05a_SDF.fps
CALL :RunIgnoreDB 05a_SDF_ZeroRowsWithAll_ignoreDB 05a_SDF.fps
CALL :RunWithDB 05b_SDF_ZeroRowsWithAll_withDB 05b_SDF.fps
CALL :RunIgnoreDB 05b_SDF_ZeroRowsWithAll_ignoreDB 05b_SDF.fps
CALL :RunWithDB 06a_SDF_ZeroRowsWithNone_withDB 06a_SDF.fps
CALL :RunIgnoreDB 06a_SDF_ZeroRowsWithNone_ignoreDB 06a_SDF.fps
CALL :RunWithDB 06b_SDF_ZeroRowsWithNone_withDB 06b_SDF.fps
CALL :RunIgnoreDB 06b_SDF_ZeroRowsWithNone_ignoreDB 06b_SDF.fps

:: Exercise Contains Exactly One Row With Any, All, None
:: 08c, 08d, 08e exercise case-sensitive and fuzzy settings
CALL :RunWithDB 07a_SDF_ExactlyOneRowWithAny_withDB 07a_SDF.fps
CALL :RunIgnoreDB 07a_SDF_ExactlyOneRowWithAny_ignoreDB 07a_SDF.fps
CALL :RunWithDB 07b_SDF_ExactlyOneRowWithAny_withDB 07b_SDF.fps
CALL :RunIgnoreDB 07b_SDF_ExactlyOneRowWithAny_ignoreDB 07b_SDF.fps
CALL :RunWithDB 07c_SDF_MissingTable_withDB 07c_SDF.fps
CALL :RunIgnoreDB 07c_SDF_MissingTable_ignoreDB 07c_SDF.fps
CALL :RunWithDB 08a_SDF_ExactlyOneRowWithAll_withDB 08a_SDF.fps
CALL :RunIgnoreDB 08a_SDF_ExactlyOneRowWithAll_ignoreDB 08a_SDF.fps
CALL :RunWithDB 08b_SDF_ExactlyOneRowWithAll_withDB 08b_SDF.fps
CALL :RunIgnoreDB 08b_SDF_ExactlyOneRowWithAll_ignoreDB 08b_SDF.fps
CALL :RunWithDB 08c_SDF_ExactlyOneRowWithAll_withDB 08c_SDF.fps
CALL :RunIgnoreDB 08c_SDF_ExactlyOneRowWithAll_ignoreDB 08c_SDF.fps
CALL :RunWithDB 08d_SDF_ExactlyOneRowWithAll_withDB 08d_SDF.fps
CALL :RunIgnoreDB 08d_SDF_ExactlyOneRowWithAll_ignoreDB 08d_SDF.fps
CALL :RunWithDB 08e_SDF_ExactlyOneRowWithAll_withDB 08e_SDF.fps
CALL :RunIgnoreDB 08e_SDF_ExactlyOneRowWithAll_ignoreDB 08e_SDF.fps
CALL :RunWithDB 09a_SDF_ExactlyOneRowWithNone_withDB 09a_SDF.fps
CALL :RunIgnoreDB 09a_SDF_ExactlyOneRowWithNone_ignoreDB 09a_SDF.fps
CALL :RunWithDB 09b_SDF_ExactlyOneRowWithNone_withDB 09b_SDF.fps
CALL :RunIgnoreDB 09b_SDF_ExactlyOneRowWithNone_ignoreDB 09b_SDF.fps

:: Exercise Query for Exactly One, Zero, At Least One Row
CALL :RunWithDB 10a_SDF_ExactlyOneRow_withDB 10a_SDF.fps
CALL :RunIgnoreDB 10a_SDF_ExactlyOneRow_ignoreDB 10a_SDF.fps
CALL :RunWithDB 10b_SDF_ExactlyOneRow_withDB 10b_SDF.fps
CALL :RunIgnoreDB 10b_SDF_ExactlyOneRow_ignoreDB 10b_SDF.fps
CALL :RunWithDB 11a_SDF_ZeroRows_withDB 11a_SDF.fps
CALL :RunIgnoreDB 11a_SDF_ZeroRows_ignoreDB 11a_SDF.fps
CALL :RunWithDB 11b_SDF_ZeroRows_withDB 11b_SDF.fps
CALL :RunIgnoreDB 11b_SDF_ZeroRows_ignoreDB 11b_SDF.fps
CALL :RunWithDB 12a_SDF_AtLeastOneRow_withDB 12a_SDF.fps
CALL :RunIgnoreDB 12a_SDF_AtLeastOneRow_ignoreDB 12a_SDF.fps
CALL :RunWithDB 12b_SDF_AtLeastOneRow_withDB 12b_SDF.fps
CALL :RunIgnoreDB 12b_SDF_AtLeastOneRow_ignoreDB 12b_SDF.fps

:::::::::::
:: Exercise the FPS files that use FAM database
:::::::::::

:: Exercise Contains Rows
CALL :RunWithDB 01_FAM_ZeroRows_withDB 01_FAM.fps
CALL :RunIgnoreDB 01_FAM_ZeroRows_ignoreDB 01_FAM_IgnoreDB.fps
CALL :RunWithDB 02a_FAM_ExactlyOneRow_withDB 02a_FAM.fps
CALL :RunIgnoreDB 02a_FAM_ExactlyOneRow_ignoreDB 02a_FAM_IgnoreDB.fps
CALL :RunWithDB 02b_FAM_ExactlyOneRow_withDB 02b_FAM.fps
CALL :RunIgnoreDB 02b_FAM_ExactlyOneRow_ignoreDB 02b_FAM_IgnoreDB.fps
CALL :RunWithDB 03a_FAM_AtLeastOneRow_withDB 03a_FAM.fps
CALL :RunIgnoreDB 03a_FAM_AtLeastOneRow_ignoreDB 03a_FAM_IgnoreDB.fps
CALL :RunWithDB 03b_FAM_AtLeastOneRow_withDB 03b_FAM.fps
CALL :RunIgnoreDB 03b_FAM_AtLeastOneRow_ignoreDB 03b_FAM_IgnoreDB.fps

:: Exercise Contains At Least One Row With Any, All, None
:: 05c, 05d exercise case-sensitive and fuzzy settings
CALL :RunWithDB 04a_FAM_AtLeastOneRowWithAny_withDB 04a_FAM.fps
CALL :RunIgnoreDB 04a_FAM_AtLeastOneRowWithAny_ignoreDB 04a_FAM_IgnoreDB.fps
CALL :RunWithDB 04b_FAM_AtLeastOneRowWithAny_withDB 04b_FAM.fps
CALL :RunIgnoreDB 04b_FAM_AtLeastOneRowWithAny_ignoreDB 04b_FAM_IgnoreDB.fps
CALL :RunWithDB 05a_FAM_AtLeastOneRowWithAll_withDB 05a_FAM.fps
CALL :RunIgnoreDB 05a_FAM_AtLeastOneRowWithAll_ignoreDB 05a_FAM_IgnoreDB.fps
CALL :RunWithDB 05b_FAM_AtLeastOneRowWithAll_withDB 05b_FAM.fps
CALL :RunIgnoreDB 05b_FAM_AtLeastOneRowWithAll_ignoreDB 05b_FAM_IgnoreDB.fps
CALL :RunWithDB 05c_FAM_AtLeastOneRowWithAll_withDB 05c_FAM.fps
CALL :RunIgnoreDB 05c_FAM_AtLeastOneRowWithAll_ignoreDB 05c_FAM_IgnoreDB.fps
CALL :RunWithDB 05d_FAM_AtLeastOneRowWithAll_withDB 05d_FAM.fps
CALL :RunIgnoreDB 05d_FAM_AtLeastOneRowWithAll_ignoreDB 05d_FAM_IgnoreDB.fps
CALL :RunWithDB 06a_FAM_AtLeastOneRowWithNone_withDB 06a_FAM.fps
CALL :RunIgnoreDB 06a_FAM_AtLeastOneRowWithNone_ignoreDB 06a_FAM_IgnoreDB.fps
CALL :RunWithDB 06b_FAM_AtLeastOneRowWithNone_withDB 06b_FAM.fps
CALL :RunIgnoreDB 06b_FAM_AtLeastOneRowWithNone_ignoreDB 06b_FAM_IgnoreDB.fps

PAUSE
GOTO :EOF

:RunWithDB
SET TestName=%1
runfpsfile %2 ..\Images\TestImageB.tif /queue /process /forceProcessing /ef "%CD%\..\Output\%TestName%_FAIL.uex"
GOTO :EOF

:RunIgnoreDB
SET TestName=%1
runfpsfile %2 ..\Images\TestImageB.tif /ignoreDB /ef "%CD%\..\Output\%TestName%_FAIL.uex"
GOTO :EOF
