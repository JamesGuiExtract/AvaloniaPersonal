@ECHO Off
ECHO Date /t >ProcessTime.txt
ECHO Time /t >>ProcessTime.txt

::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
::  Inituserenv.bat is the configuration file which contains all of the 
::  environment information
::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
CALL inituserenv.bat


::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
::  Environment Settings.

::  defines the location of the CTRSTest.bat for testing the CTRS.
SET CTRSTestHarnessLocation=%EngineeringRoot%\Engineering\ReusableComponents\Recognition\ITextRecognitionServer\AutomatedTesting\bin
::  defines the location of the CCE.bat for testing the CCE.
SET CCETestHarnessLocation=%EngineeringRoot%\Engineering\ReusableComponents\Geometry\CurveCalculationEngine\AutomatedTesting\Bin

::GOTO RunAutomatedTests
GOTO CopyHack

::This was put in place to emulate building the test drivers.
:CopyHack
XCOPY /v /s /I /r /y I:\Common\Testing\test_drivers %EngineeringRoot%


:RunAutomatedTests

::  Runs the harness fo the CTRS
cd /d %CTRSTestHarnessLocation%
CALL %CTRSTestHarnessLocation%\CTRSTest.bat


::  Runs the harness fo the CCE
cd /d %CCETestHarnessLocation%
CALL %CCETestHarnessLocation%\CCE.bat

::GOTO Done


:Done
ECHO Date /t >>ProcessTime.txt
ECHO Time /t >>ProcessTime.txt


