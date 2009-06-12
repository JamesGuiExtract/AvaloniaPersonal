@echo off

:: Turn off the following rules...
:: 
:: Microsoft.Design#CA1031 (DoNotCatchGeneralExceptionTypes)
::   Our exception handling framework explicitly requires that
:: that all exceptions thrown are thrown as ExtractExceptions
::
:: Microsoft.Performance#CA1802 (UseLiteralsWhereAppropriate)
::   We prefer static readonly constants to const constants,
:: for reasons explained in detail in the book, Effective C#.

FxCopCmd.exe /file:"%1" /console /quiet /gac ^
/ruleid:-Microsoft.Design#CA1031 ^
/ruleid:-Microsoft.Performance#CA1802