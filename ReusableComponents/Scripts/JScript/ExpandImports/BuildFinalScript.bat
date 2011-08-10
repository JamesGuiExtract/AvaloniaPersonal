:: Build JS file for use using various Extract Systems utility scripts and a
:: specific script file with a 'main' function that may use one or more utility
:: functions.
:: 
:: Usage:
::    BuildFinalScript main.js scriptname.js
:: 
:: where:
::    main.js = a 'main' function specific to this script
::    
::    scriptname.js = final script file for use
:: 
:: The customer-master.js file is built from the following order of files:
::    customer-specific.js
::    ScriptWrapper.js
:: 
copy "%1" + "..\ScriptWrapper.js" "%2" /B /Y
pause
