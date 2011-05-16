@echo off
REM Clean output files
call Clean.bat

REM Case insensitive operations
echo Performing case insensitive operations
SetOperations.exe ListB.txt union ListA.txt Out_01.txt /i
echo diffing Out_01.txt
fc .\Out_01.txt .\ExpectedOutput\Out_01.txt
pause

SetOperations.exe ListC.txt union ListA.txt Out_02.txt
echo diffing Out_02.txt
fc .\Out_02.txt .\ExpectedOutput\Out_02.txt
pause

SetOperations.exe ListA.txt Complement ListB.txt Out_03.txt /i
echo diffing Out_03.txt
fc .\Out_03.txt .\ExpectedOutput\Out_03.txt
pause

SetOperations.exe ListA.txt complement ListC.txt Out_04.txt
echo diffing Out_04.txt
fc .\Out_04.txt .\ExpectedOutput\Out_04.txt
pause

SetOperations.exe ListB.txt intersect ListA.txt Out_05.txt /i
echo diffing Out_05.txt
fc .\Out_05.txt .\ExpectedOutput\Out_05.txt
pause

SetOperations.exe ListC.txt intersect ListA.txt Out_06.txt
echo diffing Out_06.txt
fc .\Out_06.txt .\ExpectedOutput\Out_06.txt
pause

REM Case sensitive operations
echo Performing case sensitive operations
SetOperations.exe ListA.txt union ListB.txt Out_01cs.txt /c /i
echo diffing Out_01cs.txt
fc .\Out_01cs.txt .\ExpectedOutput\Out_01cs.txt
pause

SetOperations.exe ListC.txt union ListA.txt Out_02cs.txt /c
echo diffing Out_02cs.txt
fc .\Out_02cs.txt .\ExpectedOutput\Out_02cs.txt
pause

SetOperations.exe ListA.txt complement ListB.txt Out_03cs.txt /c /i
echo diffing Out_03cs.txt
fc .\Out_03cs.txt .\ExpectedOutput\Out_03cs.txt
pause

SetOperations.exe ListA.txt complement ListC.txt Out_04cs.txt /c
echo diffing Out_04cs.txt
fc .\Out_04cs.txt .\ExpectedOutput\Out_04cs.txt
pause

SetOperations.exe ListA.txt intersect ListB.txt Out_05cs.txt /c /i
echo diffing Out_05cs.txt
fc .\Out_05cs.txt .\ExpectedOutput\Out_05cs.txt
pause

SetOperations.exe ListA.txt intersect ListC.txt Out_06cs.txt /c
echo diffing Out_06cs.txt
fc .\Out_06cs.txt .\ExpectedOutput\Out_06cs.txt
pause


REM Test error handling
echo Begining error test cases - ready?
pause
echo Check current working dir for SetOperationsError.uex with current timestamp
SetOperations.exe ListA.txt union ListB.txt /c /ef .\SetOperationsError.uex

echo Finished!
pause
