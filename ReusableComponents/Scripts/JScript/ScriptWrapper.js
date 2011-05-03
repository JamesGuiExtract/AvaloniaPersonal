//--------------------------------------------------------------------------------------------------
// Extract Systems Utility Subroutines
//--------------------------------------------------------------------------------------------------
// Define default error display and debug settings
var GBoolLogErrorsOnly = false;
var GBoolShowDebugInfo = false;

// Run the script
try {
    main(parseCommandLineOptions());
}
catch(err) {
    handleScriptError("ELI32515", "Fatal Error", err);
}

//--------------------------------------------------------------------------------------------------
// Parses the command-line arguments for options and returns the remaining arguments.
// Will set one or more of the following global boolean parameters.
//    GBoolShowDebugInfo ===> -debug
//    GBoolLogErrorsOnly ===> -silent
//--------------------------------------------------------------------------------------------------
function parseCommandLineOptions() {
    var args = WScript.Arguments;
    var filteredArgs = [];
    var strArg;
    for (var i=0; i < args.length; i++) {
        strArg = args(i);
        if (/^[-\/]silent$/i.test(strArg)) {
            GBoolLogErrorsOnly = true;
        }
        else if (/^[-\/]debug$/i.test(strArg)) {
            GBoolShowDebugInfo = true;
        }
        else {
            filteredArgs.push(strArg);
        }
    }
    return filteredArgs;
}

//--------------------------------------------------------------------------------------------------
// Handles errors by wrapping them in a COMUCLIDException object,
// logging them and, depending on a command-line setting, displaying them.
// Required arguments: strELI, strText, oErr
// Optional arguments: any number of name/value pairs of debug info
//--------------------------------------------------------------------------------------------------
function handleScriptError() {
    var strELI = arguments[0];
    var strText = arguments[1];
    var oErr = arguments[2];

    // Create inner exception object
    var innerExceptionObject = new ActiveXObject("UCLIDExceptionMgmt.COMUCLIDException");
    innerExceptionObject.createFromString(strELI, strText);

    // Create script exception with the inner exception
    var exceptionObject = new ActiveXObject("UCLIDExceptionMgmt.COMUCLIDException");
    exceptionObject.createWithInnerException("ELI32516", "Script Exception", innerExceptionObject);

    // Add debug records with error number and description
    exceptionObject.addDebugInfo("Err.Number", oErr.number);
    exceptionObject.addDebugInfo("Err.Description", oErr.description);

    // Add debug record for all name/value pairs
    for (var i=3; i < arguments.length-1; i+=2) {
        exceptionObject.AddDebugInfo(arguments[i], arguments[i+1]);
    }

    // Display the exception if desired
    if (!GBoolLogErrorsOnly) {
        exceptionObject.display();
    }

    // Always log the exception
    exceptionObject.log();
}

//--------------------------------------------------------------------------------------------------
// Logs specified error information by wrapping it in a COMUCLIDException object and 
// logging it.
// Required arguments: strELI, strText, oErr
// Optional arguments: any number of name/value pairs of debug info
//--------------------------------------------------------------------------------------------------
function logScriptError() {
    var strELI = arguments[0];
    var strText = arguments[1];
    var oErr = arguments[2];

    // Create inner exception object
    var innerExceptionObject = new ActiveXObject("UCLIDExceptionMgmt.COMUCLIDException");
    innerExceptionObject.createFromString(strELI, strText);

    // Create script exception with the inner exception
    var exceptionObject = new ActiveXObject(UCLIDExceptionMgmt.COMUCLIDException);
    exceptionObject.createWithInnerException("ELI32517", "Logged Script Exception",
            innerExceptionObject);

    // Add debug records with error number and description
    exceptionObject.addDebugInfo("Err.Number", oErr.number);
    exceptionObject.addDebugInfo("Err.Description", oErr.description);

    // Add debug record for all name/value pairs
    for (var i=3; i < arguments.length-1; i+=2) {
        exceptionObject.AddDebugInfo(arguments[i], arguments[i+1]);
    }

    // log the exception
    exceptionObject.log();
}

//--------------------------------------------------------------------------------------------------
// Central handler for Debug information where nothing is displayed to the user unless 
// "-debug" was included on the command line
// Will display a message box with "Script Debug Information" title
// and any key/value pairs passed as distinct arguments or as an array
//--------------------------------------------------------------------------------------------------
function handleDebug() {
    // Check global setting and return if debugging is disabled
    if (!GBoolShowDebugInfo) {
        return;
    }
    var pairs = [];
    if (arguments.length == 1) {
        pairs = arguments[0];
    }
    else {
        for (i=0; i < arguments.length-1; i+=2) {
            pairs.push(arguments[i] + ": " + arguments[i+1]);
        }
    }
    (new ActiveXObject("WScript.Shell")).
        popup(pairs.join("\n"), 0, "Script Debug Information");
}
//--------------------------------------------------------------------------------------------------
