:: Build JS file for use using various Extract Systems utility scripts and a
:: specific script file with a 'main' function that may use one or more utility
:: functions.
:: 
:: Usage:
::    BuildFinalScript
:: 
:: where:
::    main.js = a 'main' function specific to this script
:: 
:: The master.js file is built from the following order of files:
::    specific.js
::    ScriptWrapper.js
::
:: The master.js file is named after the batch file's parent dir

for /f "delims=\" %%a in ("%~dp0") do set name=%%~nxa.js
copy "main.js" + "%~dp0..\Common\ScriptWrapper.js" "..\%name%" /B /Y
