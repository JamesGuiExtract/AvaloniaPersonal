echo off
rem Create Tutorial EXE's
rem Author: Wayne Lenius
rem Date:   01/08/03
rem 
rem VB Process:
rem 1. Use ATTRIB -R <filename> to remove read-only flag from files 
rem 2. Use ReplaceString.exe to replace InitializeFromFile() 
rem    with Initialize() for license-aware tutorials
rem 3. Build the VB Tutorials
rem 
rem VC++ Process:
rem 1. Use ATTRIB -R <filename> to remove read-only flag from files 
rem 2. Use ReplaceString.exe to replace InitializeFromFile() 
rem    with Initialize() for license-aware tutorials
rem 3. Use ReplaceString.exe to replace /I /Bin include directory 
rem    option with /Binaries/Release
rem 4. Build the VC++ Tutorials
rem 
rem NOTES:
rem 1. The ReplaceString utility application is used for license and
rem    option replacement
rem 2. Note the different relative path needed for the first CD 
rem    (Change Directory) command
rem 3. The files to be modified must not be read-only
rem 4. DLL-based projects are not built at this time
rem

if not exist BuildEXEs.bat goto location_error
rem Tutorial VB1
	CD "..\VB1\Code (Final)"

	ATTRIB -R Form1.frm

	..\..\..\..\..\Utils\ReplaceString\Release\ReplaceString.exe Form1.frm "InitializeFromFile \"INSERT FILENAME HERE\", 1, 2, 3, 4" "Initialize \"AW247YHUG8\""

	VB6 /make Project1.vbp

rem Tutorial VB2
	CD "..\..\VB2\Code (Final)"

	ATTRIB -R Form1.frm

	..\..\..\..\..\Utils\ReplaceString\Release\ReplaceString.exe Form1.frm "InitializeFromFile \"INSERT FILENAME HERE\", 1, 2, 3, 4" "Initialize \"AW247YHUG8\""

	VB6 /make Project1.vbp

rem Tutorial VB3
rem *** No license-related changes needed for TutorialVB3
	CD "..\..\VB3\Code (Final)"

	ATTRIB -R CLTestIR.dll

	VB6 /make CLTestIR.vbp

rem Tutorial VB2 plus VB3
	CD "..\..\VB2\Code (Final plus VB3)"

	ATTRIB -R Form1.frm

	..\..\..\..\..\Utils\ReplaceString\Release\ReplaceString.exe Form1.frm "InitializeFromFile \"INSERT FILENAME HERE\", 1, 2, 3, 4" "Initialize \"AW247YHUG8\""

	VB6 /make Project1.vbp

rem Tutorial VB4
rem *** No license-related changes needed for TutorialVB4
	CD "..\..\VB4\Code (Final)"

	ATTRIB -R ParcelIDValidator.dll

	VB6 /make ParcelIDValidator.vbp

rem Tutorial VB5
	CD "..\..\VB5\Code (Final)"

	ATTRIB -R Form1.frm

	..\..\..\..\..\Utils\ReplaceString\Release\ReplaceString.exe Form1.frm "InitializeFromFile \"INSERT FILENAME HERE\", 1, 2, 3, 4" "Initialize \"AW247YHUG8\""

	VB6 /make Project1.vbp

rem Tutorial VB6
	CD "..\..\VB6\Code (Final)"

	ATTRIB -R Form1.frm

	..\..\..\..\..\Utils\ReplaceString\Release\ReplaceString.exe Form1.frm "InitializeFromFile \"INSERT FILENAME HERE\", 1, 2, 3, 4" "Initialize \"AW247YHUG8\""

	VB6 /make Project1.vbp

rem Tutorial VB7
	CD "..\..\VB7\Code (Final)"

	ATTRIB -R Form1.frm

	..\..\..\..\..\Utils\ReplaceString\Release\ReplaceString.exe Form1.frm "InitializeFromFile \"INSERT FILENAME HERE\", 1, 2, 3, 4" "Initialize \"AW247YHUG8\""

	VB6 /make Project1.vbp

rem Tutorial VB8
	CD "..\..\VB8\Code (Final)"

	ATTRIB -R Form1.frm

	..\..\..\..\..\Utils\ReplaceString\Release\ReplaceString.exe Form1.frm "InitializeFromFile \"INSERT FILENAME HERE\", 1, 2, 3, 4" "Initialize \"AW247YHUG8\""

	VB6 /make Project1.vbp

rem Tutorial VB9
	CD "..\..\VB9\Code (Final)"

	ATTRIB -R Form1.frm

	..\..\..\..\..\Utils\ReplaceString\Release\ReplaceString.exe Form1.frm "InitializeFromFile \"INSERT FILENAME HERE\", 1, 2, 3, 4" "Initialize \"AW247YHUG8\""

	VB6 /make Project1.vbp

rem
rem Tutorial VC1
	CD "..\..\VC1\Code (Final)"

rem *** No license-related changes needed for TutorialVC1
rem
	msdev TutorialVC1.dsp /MAKE "TutorialVC1 - Win32 Release" /REBUILD

rem Tutorial VC2
	CD "..\..\VC2\Code (Final)"

	ATTRIB -R TutorialVC2.dsp

	..\..\..\..\..\Utils\ReplaceString\Release\ReplaceString.exe TutorialVC2.dsp "/I \"..\..\..\Bin\"" "/I \"..\..\..\..\..\..\Binaries\Release\""

	ATTRIB -R TutorialVC2Dlg.cpp

	..\..\..\..\..\Utils\ReplaceString\Release\ReplaceString.exe TutorialVC2Dlg.cpp "InitializeFromFile( \"Insert filename here!!!\", 1, 2, 3, 4 );" "Initialize( \"AW247YHUG8\" );"

	msdev TutorialVC2.dsp /MAKE "TutorialVC2 - Win32 Release" /REBUILD

rem Tutorial VC3
	CD "..\..\VC3\Code (Final)"

	ATTRIB -R TutorialVC3.dsp

	..\..\..\..\..\Utils\ReplaceString\Release\ReplaceString.exe TutorialVC3.dsp "/I \"..\..\..\Bin\"" "/I \"..\..\..\..\..\..\Binaries\Release\""

	ATTRIB -R TutorialVC3Dlg.cpp

	..\..\..\..\..\Utils\ReplaceString\Release\ReplaceString.exe TutorialVC3Dlg.cpp "InitializeFromFile( \"Insert filename here!!!\", 1, 2, 3, 4 );" "Initialize( \"AW247YHUG8\" );"

	msdev TutorialVC3.dsp /MAKE "TutorialVC3 - Win32 Release" /REBUILD

rem Tutorial VC4
	CD "..\..\VC4\Code (Final)"

rem *** No license-related changes needed for TutorialVC4
rem
	ATTRIB -R ParcelIDFinder.dsp

	..\..\..\..\..\Utils\ReplaceString\Release\ReplaceString.exe ParcelIDFinder.dsp "/I \"..\..\..\Bin\"" "/I \"..\..\..\..\..\..\Binaries\Release\""

	msdev ParcelIDFinder.dsp /MAKE "ParcelIDFinder - Win32 Release" /REBUILD

echo on
rem Finished creating Tutorial EXE's

pause
goto done

:location_error
echo ERROR! Batch file is being run from the wrong directory!

:done
