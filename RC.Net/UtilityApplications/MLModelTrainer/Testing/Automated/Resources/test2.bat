@ECHO OFF
SETLOCAL

ECHO Evaluated 51818 samples with 49 entities; found: 45 entities; correct: 43.
ECHO       TOTAL: precision:   95.56%%;  recall:   87.76%%; F1:   91.49%%.
ECHO       Party: precision:   95.56%%;  recall:   87.76%%; F1:   91.49%%. [target:  49; tp:  43; fp:   2]

SET /p Train=< %1
ECHO Training Result:> %1
ECHO %Train%>> %1
