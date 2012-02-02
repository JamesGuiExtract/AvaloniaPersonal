//--------------------------------------------------------------------------------------------------
// Extract Systems Utility Subroutines
//--------------------------------------------------------------------------------------------------
// Define default error display and debug settings
var GBoolLogErrorsOnly = false;
var GBoolShowDebugInfo = false;

// Create FileSystemObject
var fso = new ActiveXObject("Scripting.FileSystemObject");

//--------------------------------------------------------------------------------------------------
// Add trim function to String
//--------------------------------------------------------------------------------------------------
if(typeof String.prototype.trim !== 'function') {
  String.prototype.trim = function() {
    return this.replace(/^\s+|\s+$/g, '');
  };
}

//--------------------------------------------------------------------------------------------------
// Add map function to Array
//--------------------------------------------------------------------------------------------------
if(typeof Array.prototype.map !== 'function') {
	Array.prototype.map = function(fn) {
		for (var i=0, r=[], l = this.length; i < l; r.push(fn(this[i++])));
			return r;
	};
}

//--------------------------------------------------------------------------------------------------
// Add filter function to Array
//--------------------------------------------------------------------------------------------------
if(typeof Array.prototype.filter !== 'function') {
  Array.prototype.filter = function(fn /*, thisp*/) {
    var len = this.length;
    if (typeof fn != "function")
      throw new TypeError();

    var res = new Array();
    var thisp = arguments[1];
    for (var i = 0; i < len; i++) {
      if (i in this) {
        var val = this[i]; // in case fun mutates this
        if (fn.call(thisp, val, i, this)) {
          res.push(val);
        }
      }
    }
    return res;
  };
}

//--------------------------------------------------------------------------------------------------
// Add has function to Array
//--------------------------------------------------------------------------------------------------
if(typeof Array.prototype.has !== 'function') {
	Array.prototype.has = function(value) {
    for (var i=0, l = this.length; i < l; i++) {
      if (this[i] == value) {
        return true;
      }
    }
    return false;
  };
}

// Run the script
main(parseCommandLineOptions());

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
        for (var i=0; i < arguments.length-1; i+=2) {
            pairs.push(arguments[i] + ": " + arguments[i+1]);
        }
    }
    (new ActiveXObject("WScript.Shell")).
        popup(pairs.join("\n"), 0, "Script Debug Information");
}

//--------------------------------------------------------------------------------------------------
// Save an XML document with pretty indentation
//--------------------------------------------------------------------------------------------------
function prettyXMLSave(xDoc, strFileName) {
    handleDebug("Entering prettyXMLSave()", strFileName);

    var rdr = new ActiveXObject("MSXML2.SAXXMLReader");
    var wrt = new ActiveXObject("MSXML2.MXXMLWriter");
    var oStream = new ActiveXObject("ADODB.STREAM");
    oStream.open();
    oStream.charset = "ISO-8859-1";

    wrt.indent = true;
    wrt.omitXMLDeclaration = true;
    wrt.encoding = "ISO-8859-1";
    wrt.output = oStream;
    rdr.contentHandler = wrt;
    rdr.errorHandler = wrt;
    rdr.parse(xDoc);
    wrt.flush();

    try {
        // Save, overwriting if present, creating if not
        oStream.saveToFile(strFileName, 2);
    }
    catch(err) {
        handleScriptError("ELI32526", "Unable to save XML file!", err, "XML Path", strFileName);
    }
}

//--------------------------------------------------------------------------------------------------
// Read all text from the file
//--------------------------------------------------------------------------------------------------
function readAllText(fname) {
    // Open the file
    try {
        var f = fso.OpenTextFile(fname, 1);
    }
    catch(err) {
        handleScriptError("ELI33180", "Unable to open input file!", err, "FileName", fname);
    }
    // Read from the file
    if (f.AtEndOfStream)
        return ("");
    else
        return (f.ReadAll());
}

//--------------------------------------------------------------------------------------------------
// Write all text to the file
//--------------------------------------------------------------------------------------------------
function writeText(fname, text) {
    // Open the file
    try {
        var f = fso.OpenTextFile(fname, 2, true);
    }
    catch(err) {
        handleScriptError("ELI33181", "Unable to open output file!", err, "FileName", fname);
    }
    // Write to the file
    f.Write(text);
    f.Close();
}

//--------------------------------------------------------------------------------------------------
// Get array of file objects from a directory name
//--------------------------------------------------------------------------------------------------
function getFiles(dirname, recursive) {
  var ret = [];

  _getFiles(dirname);

  function _getFiles(dirname) {
     var folder = fso.GetFolder(dirname);
     // Get Files in current directory  
     var files = new Enumerator(folder.files);
     // Loop through files  
     for(; !files.atEnd(); files.moveNext()) {
        ret.push(files.item());
     }

     if (recursive) {
       var subfolders = new Enumerator(folder.SubFolders);

       for(; !subfolders.atEnd(); subfolders.moveNext()) {
          _getFiles(subfolders.item().Path);
       }
     }
  }
  return ret;
}

//--------------------------------------------------------------------------------------------------
