@echo off
REM Clean output files
call Clean.bat

REM Case insensitive operations
echo Performing case insensitive operations
SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a+b Out_01.txt
echo diffing Out_01.txt
fc .\Out_01.txt .\ExpectedOutput\Out_01.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a+c Out_02.txt
echo diffing Out_02.txt
fc .\Out_02.txt .\ExpectedOutput\Out_02.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a-b Out_03.txt
echo diffing Out_03.txt
fc .\Out_03.txt .\ExpectedOutput\Out_03.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a-c Out_04.txt
echo diffing Out_04.txt
fc .\Out_04.txt .\ExpectedOutput\Out_04.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a*b Out_05.txt
echo diffing Out_05.txt
fc .\Out_05.txt .\ExpectedOutput\Out_05.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a*c Out_06.txt
echo diffing Out_06.txt
fc .\Out_06.txt .\ExpectedOutput\Out_06.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a+b-c Out_07.txt
echo diffing Out_07.txt
fc .\Out_07.txt .\ExpectedOutput\Out_07.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a+c-b Out_08.txt
echo diffing Out_08.txt
fc .\Out_08.txt .\ExpectedOutput\Out_08.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt (a+b)*c Out_09.txt
echo diffing Out_09.txt
fc .\Out_09.txt .\ExpectedOutput\Out_09.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt (a+b)*c Out_10.txt
echo diffing Out_10.txt
fc .\Out_10.txt .\ExpectedOutput\Out_10.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt (a-b)*(a+c) Out_11.txt
echo diffing Out_11.txt
fc .\Out_11.txt .\ExpectedOutput\Out_11.txt
pause


REM Case sensitive operations
echo Performing case sensitive operations
SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a+b Out_01cs.txt /c
echo diffing Out_01cs.txt
fc .\Out_01cs.txt .\ExpectedOutput\Out_01cs.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a+c Out_02cs.txt /c
echo diffing Out_02cs.txt
fc .\Out_02cs.txt .\ExpectedOutput\Out_02cs.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a-b Out_03cs.txt /c
echo diffing Out_03cs.txt
fc .\Out_03cs.txt .\ExpectedOutput\Out_03cs.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a-c Out_04cs.txt /c
echo diffing Out_04cs.txt
fc .\Out_04cs.txt .\ExpectedOutput\Out_04cs.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a*b Out_05cs.txt /c
echo diffing Out_05cs.txt
fc .\Out_05cs.txt .\ExpectedOutput\Out_05cs.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a*c Out_06cs.txt /c
echo diffing Out_06cs.txt
fc .\Out_06cs.txt .\ExpectedOutput\Out_06cs.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a+b-c Out_07cs.txt /c
echo diffing Out_07cs.txt
fc .\Out_07cs.txt .\ExpectedOutput\Out_07cs.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt a+c-b Out_08cs.txt /c
echo diffing Out_08cs.txt
fc .\Out_08cs.txt .\ExpectedOutput\Out_08cs.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt (a+b)*c Out_09cs.txt /c
echo diffing Out_09cs.txt
fc .\Out_09cs.txt .\ExpectedOutput\Out_09cs.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt (a+b)*c Out_10cs.txt /c
echo diffing Out_10cs.txt
fc .\Out_10cs.txt .\ExpectedOutput\Out_10cs.txt
pause

SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt (a-b)*(a+c) Out_11cs.txt /c
echo diffing Out_11cs.txt
fc .\Out_11cs.txt .\ExpectedOutput\Out_11cs.txt
pause


REM Test error handling
echo Begining error test cases - ready?
pause
echo Check for mismatched grouping error - begin without end
SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt ((a+b)*c Out_Error.txt
echo Check for mismatched grouping error - end without begin
SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt (a+b)*c) Out_Error.txt
echo Check for invalid expression error - operand without operator error
SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt (((a+b)*c)b) Out_Error.txt
echo Check for operator with invalid number of arguments error
SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt ((a++b)*c) Out_Error.txt
echo Check for duplicate in list error
SetOp a=ListA.txt,b=ListInvalid.txt a+b Out_Error.txt


echo
echo Check C:\temp\ for SetOpError.uex with current timestamp
SetOp a=ListA.txt,b=ListB.txt,c=ListC.txt ((a+b)*c Out_Error.txt /ef C:\temp\SetOpError.uex
pause

echo Finished!
pause
