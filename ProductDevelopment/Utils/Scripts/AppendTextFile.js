//--------------------------------------------------------------------------------------------------
// Script commands specific for AppendTextFile
//--------------------------------------------------------------------------------------------------
//
//--------------------------------------------------------------------------------------------------
// Script Description
//--------------------------------------------------------------------------------------------------
// Appends the contents of one text file to a second text file
//
// Script arguments:
//              firstFile            - The full path of a text file to append from
//              secondFile           - The full path of a text file to append to
//              insertMissingNewline - true or false (true by default)
//                                     Optional flag to control whether a newline is inserted
//                                     between the existing text of secondFile and the text of
//                                     firstFile if neither firstFile starts with a newline nor
//                                     secondFile ends with a newline
//
//--------------------------------------------------------------------------------------------------

function main(args) {
    handleDebug("Command-Line Arguments", args.join(","));

    var firstFile = args[0];
    var secondFile = args[1];
    if (args.length > 2 && args[2].toUpperCase() == "FALSE") {
        var insertMissingNewline = false;
    } else {
        var insertMissingNewline = true;
    }

    //--------------------------------------------------------------------------
    // Append first file to second file
    //--------------------------------------------------------------------------
    try {
        var text = readAllText(firstFile);
    }
    catch(err) {
        handleScriptError("ELI37411", "Unable to read from text file!", err, "File name", firstFile);
        throw err;
    }
    appendText(secondFile, text, insertMissingNewline);

    // Function to append text to a file
    function appendText(fname, text, insertMissingNewline) {

        // Create named Mutex Object to protect access to the output file
        var mutex = new ActiveXObject("UCLIDCOMUtils.COMMutex");
        try {
            mutex.createNamed("AppendTextFile_"+fso.GetFileName(fname));
        }
        catch(err) {
            handleScriptError("ELI37412", "Unable to create COMMutex object!", err);
        }

        // Aquire the mutex
        var haveMutex = false;
        var attempts = 0;

        // Try to get the mutex up to 1000 times before exception
        try {
            while(haveMutex == false) {
                attempts++;
                try {
                    mutex.acquire();
                    haveMutex = true;
                }
                catch(err) {
                    if (attempts == 1000) { throw err; }
                    WScript.Sleep(200);
                }
            }
        }
        catch(err) { handleScriptError("ELI37413", "Unable to acquire named Mutex!", err); throw err; }
        try {

            // Set flag for appending newline
            // If insertMissingNewline==true and second file exists but does not end with a newline
            // then set appendNewline=true
            var appendNewline = false;
            if (insertMissingNewline && fso.fileExists(fname)) {

                // Open the file for reading
                try {
                    var f = fso.OpenTextFile(fname, 1, true);
                }
                catch(err) { handleScriptError("ELI37414", "Unable to open output file for reading!", err, "FileName", fname); throw err; }

                // Check last line of file
                var lastchar = "";
                while (!f.AtEndOfStream) {
                    lastchar = f.read(1);
                }
                if (lastchar != "\n") {
                    appendNewline = true;
                }
                f.Close();
            }

            // Open the file
            var isOpen = false;
            var attempts = 0;

            // Try to open file up to 1000 times before exception
            try {
                while(isOpen == false) {
                    attempts++;
                    try {
                        var f = fso.OpenTextFile(fname, 8, true);
                        isOpen = true;
                    }
                    catch(err) {
                        if (attempts == 1000) { throw err; }
                        WScript.Sleep(200);
                    }
                }
            }
            catch(err) { handleScriptError("ELI37415", "Unable to open output file for writing!", err, "FileName", fname); throw err; }

            // Write to the file
            if (appendNewline) {
                text = "\r\n"+text;
            }
            f.Write(text);
            f.Close();
        }
        finally {
            // Release the Mutex
            try {
                mutex.releaseNamedMutex();
            }
            catch(err) {
                handleScriptError("ELI37416", "Unable to release named Mutex!", err);
            }
        }
    }
}
// Run the WOW64 wscript if OS is 64 bit

var shell = WScript.CreateObject("WScript.Shell");
var cpu = shell.ExpandEnvironmentStrings("%PROCESSOR_ARCHITECTURE%").toLowerCase();
var host = WScript.FullName.toLowerCase();

// check to see if we are on an AMD64 processor and running the
// wrong version of the scripting host.

if(host.indexOf("system32") != -1 && cpu == "amd64") {
    var syswow64Host = host.replace(/system32/g, "syswow64");
    var newCmd = syswow64Host + " \"" +
        WScript.ScriptFullName + "\" //Nologo";

    // ATTEMPT to pass all the same command
    //  line args to the new process

    var args = WScript.Arguments;
    for(i=0; i<args.length; i++)
        newCmd += " \"" + args(i) + "\"";

//    WScript.Echo("Running the syswow64 bit version instead...\n  " + newCmd + "\n");

    // launch the new script and echo all the output
    var exec = shell.Exec(newCmd);
    while(exec.Status == 0) {
        if(!exec.StdOut.AtEndOfStream)
            WScript.Echo(exec.StdOut.ReadAll());
        WScript.Sleep(100);
    }

    if(!exec.StdOut.AtEndOfStream)
        WScript.Echo(exec.StdOut.ReadAll());

    WScript.Quit(exec.ExitCode);
}

//--------------------------------------------------------------------------------------------------
// Extract Systems Utility Subroutines
//--------------------------------------------------------------------------------------------------
// Define default error display and debug settings
var GBoolLogErrorsOnly = false;
var GBoolSaveErrors = false;
var GStrExceptionFileName;
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
try {
    main(parseCommandLineOptions());
}
catch(err) {
    if (GBoolSaveErrors) {
        handleScriptError("ELI35991", "Unhandled Error", err);
    }
    else {
        throw err;
    }
}

//--------------------------------------------------------------------------------------------------
// Parses the command-line arguments for options and returns the remaining arguments.
// Will set one or more of the following global boolean parameters.
//    GBoolShowDebugInfo ===> -debug
//    GBoolLogErrorsOnly ===> -silent
//    GBoolLogErrorsOnly, GBoolSaveErrors and GStrExceptionFileName ===> /ef <ExceptionFileName>
//--------------------------------------------------------------------------------------------------
function parseCommandLineOptions() {
    var args = WScript.Arguments;
    var filteredArgs = [];
    var strArg;
    var nextIsErrorFileName = false;
    for (var i=0; i < args.length; i++) {
        strArg = args(i);
        if (/^[-\/]ef$/i.test(strArg)) {
            GBoolSaveErrors = true;
            nextIsErrorFileName = true;
        }
        else if (nextIsErrorFileName) {
            GBoolLogErrorsOnly = true;
            GStrExceptionFileName = strArg;
            nextIsErrorFileName = false;
        }
        else if (/^[-\/]silent$/i.test(strArg)) {
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

    // Save error if desired
    try {
        if (GBoolSaveErrors) {
            exceptionObject.SaveTo(GStrExceptionFileName, false);
        }
    }
    catch (error) {}

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
    try {
        // Read from the file
        if (f.AtEndOfStream)
            return ("");
        else
            return (f.ReadAll());
    }
    finally {
        f.Close();
    }
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
    try {
        f.Write(text);
    }
    finally {
        f.Close();
    }
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
// Get the name of a temporary file
//--------------------------------------------------------------------------------------------------
function getTempFilePath()
{
   var TemporaryFolder = 2;
   return fso.BuildPath(fso.GetSpecialFolder(TemporaryFolder).Path, fso.GetTempName());
}

//--------------------------------------------------------------------------------------------------
