@echo off

:: Turn off the following rules...
:: 
:: Microsoft.Design#CA1031 (DoNotCatchGeneralExceptionTypes)
::   Our exception handling framework explicitly requires that
::   that all exceptions thrown are thrown as ExtractExceptions
::
:: Microsoft.Design#CA1053 (StaticHolderTypesShouldNotHaveConstructors)
::   NUnit requires a static class with a default constructor.
::
:: Microsoft.Naming#CA1707 (IdentifiersShouldNotContainUnderscores)
::   Extract naming guidelines for grouping Automated and Interactive test
::   cases conflict with this guideline.
::
:: Microsoft.Naming#CA1711 (IdentifiersShouldNotHaveIncorrectSuffix)
::   Extract naming guidelines for unit test class conflict with this guideline.
::
:: Microsoft.Performance#CA1802 (UseLiteralsWhereAppropriate)
::   We prefer static readonly constants to const constants,
::   for reasons explained in detail in the book, Effective C#.
::
:: Microsoft.Design#CA2210 (AssembliesShouldHaveValidStrongNames)
::   This assembly is not being distributed and has no need to be strong-named.

FxCopCmd.exe /file:"%1" /console /quiet /gac ^
/ruleid:-Microsoft.Design#CA1031 ^
/ruleid:-Microsoft.Design#CA1053 ^
/ruleid:-Microsoft.Naming#CA1707 ^
/ruleid:-Microsoft.Naming#CA1711 ^
/ruleid:-Microsoft.Performance#CA1802 ^
/ruleid:-Microsoft.Design#CA2210
