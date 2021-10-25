//--------------------------------------------------------------------------------------------------
// Script commands specific for UpdateFKB
//--------------------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------------------
// License the SDK
//--------------------------------------------------------------------------------------------------
function license(version) {
    var lm = new ActiveXObject("UCLIDCOMLM.UCLIDComponentLM");
    var WSHShell = new ActiveXObject("WScript.Shell");
    if (version == 9) {
        var aupdir = WSHShell.ExpandEnvironmentStrings("%ALLUSERSPROFILE%");
        var licenseFilesDir = fso.BuildPath(aupdir, "Application Data\\Extract Systems\\LicenseFiles");
    }
    else {
      if (WSHShell.ExpandEnvironmentStrings("%ProgramFiles(x86)%") == "%ProgramFiles(x86)%") {
          var licenseFilesDir = "C:\\Program Files\\Extract Systems\\CommonComponents";
      }
      else {
          var licenseFilesDir = "C:\\Program Files (x86)\\Extract Systems\\CommonComponents";
      }
    }
    var licFiles = getFiles(licenseFilesDir, false).filter(
        function(f) {
            if (!f.Path.match(/\.lic$/i)) {
                return false;
            }
            return fso.fileExists(f.Path.replace(/\.lic$/i, ".pwd"));
        }
      );

    for (var i=0; i < licFiles.length; i++) {
      var lic = licFiles[i].Path;
      var pwd = lic.replace(/\.lic$/i, ".pwd");
      var pwd_keys = readAllText(pwd).split(/[, \r\n]/);
      lm.InitializeFromFile(lic,parseInt(pwd_keys[0]),parseInt(pwd_keys[1]),parseInt(pwd_keys[2]),parseInt(pwd_keys[3]));
    }
}

function main(args) {
    // License version 9+ of the software
    // Assumes proper license and password file exist
    try {
        license(9);
    }
    catch (err) {
        handleScriptError("ELI37749", "Unable to license SDK (version 9+)", err);
    }

    var root = fso.getAbsolutePathName(args[0]);
    var newFKB = args[1].replace(/^FKB\sVer\.\s*/i, "");
    if (newFKB === undefined || newFKB == "") {
        throw new Error(1000000, "Must specify non-blank FKB!");
    }

    var rsdFiles = getFiles(root, true).filter(function(f){return f.Name.match(/\.rsd$/i)});

    for (var i=0; i < rsdFiles.length; i++) {
        var ruleset = new ActiveXObject("UCLIDAFCore.RuleSet");
        var rsdfilename = rsdFiles[i];
        try {
            ruleset.LoadFrom(rsdfilename, true);
        }
        catch (err) {
            handleScriptError("ELI37750", "Unable to load ruleset", err, "RSD File", rsdfilename);
            continue;
        }

        var modified = false;

        if (ruleset.FKBVersion != "" && ruleset.FKBVersion != newFKB) {
            ruleset.FKBVersion = newFKB;
            modified = true;
        }
        if (modified) {
            try {
                ruleset.SaveTo(rsdfilename, true);
            }
            catch (err) {
                handleScriptError("ELI37751", "Unable to save ruleset", err, "RSD File", rsdfilename);
            }
        }
    }

    // Update FKBVersion setting in any order mapping DB
    var WSHShell = new ActiveXObject("WScript.Shell");

    var exporterPath = "";

    // For when using on build machine
    if (WSHShell.ExpandEnvironmentStrings("%BUILD_PRODUCT_RELEASE%") != "%BUILD_PRODUCT_RELEASE%") {
        exporterPath = WSHShell.ExpandEnvironmentStrings("%BUILD_VSS_ROOT%\\Engineering\\Binaries\\Release\\SqlCompactExporter.exe");
    }

    // For when using on dev machine
    if (!fso.fileExists(exporterPath)) {
        exporterPath = WSHShell.ExpandEnvironmentStrings("C:\\Engineering\\Binaries\\Debug\\SqlCompactExporter.exe");
    }

    // For when using on rule writer machine
    if (!fso.fileExists(exporterPath)) {
        if (WSHShell.ExpandEnvironmentStrings("%ProgramFiles(x86)%") == "%ProgramFiles(x86)%") {
            exporterPath = "C:\\Program Files\\Extract Systems\\CommonComponents\\SQLCompactExporter.exe";
        } else {
            exporterPath = "C:\\Program Files (x86)\\Extract Systems\\CommonComponents\\SQLCompactExporter.exe";
        }
    }

    if (fso.fileExists(exporterPath)) {
        var orderMappingDBs = getFiles(root, true).filter(function(f){return f.Name.match(/^OrderMappingDB\.(sdf|sqlite)$/i)});
        for (var i=0; i < orderMappingDBs.length; i++) {
            var databasePath = orderMappingDBs[i];
            var exporter = "\"" + exporterPath + "\" \"" + databasePath + "\"";
            args = "\"UPDATE [Settings] SET [Value] = '" + newFKB + "' WHERE [Name] = 'FKBVersion'\" _";

            // Run EXE and wait on return
            WSHShell.Run(exporter + " " + args, 0, true);
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

//--------------------------------------------------------------------------------------------------
// Add some function to Array
//--------------------------------------------------------------------------------------------------
if (typeof Array.prototype.some !== 'function') {

  // Production steps of ECMA-262, Edition 5, 15.4.4.17
  // Reference: http://es5.github.io/#x15.4.4.17
  Array.prototype.some = function(fun /*, thisArg*/) {
    if (this == null) {
      throw new TypeError('Array.prototype.some called on null or undefined');
    }

    if (typeof fun !== 'function') {
      throw new TypeError();
    }

    var t = Object(this);
    var len = t.length >>> 0;

    var thisArg = arguments.length >= 2 ? arguments[1] : void 0;
    for (var i = 0; i < len; i++) {
      if (i in t && fun.call(thisArg, t[i], i, t)) {
        return true;
      }
    }

    return false;
  };
}

//--------------------------------------------------------------------------------------------------
// Add some function to Array
//--------------------------------------------------------------------------------------------------
if (typeof Array.prototype.every !== 'function') {
  Array.prototype.every = function(callbackfn, thisArg) {
    var T, k;

    if (this == null) {
      throw new TypeError('this is null or not defined');
    }

    // 1. Let O be the result of calling ToObject passing the this 
    //    value as the argument.
    var O = Object(this);

    // 2. Let lenValue be the result of calling the Get internal method
    //    of O with the argument "length".
    // 3. Let len be ToUint32(lenValue).
    var len = O.length >>> 0;

    // 4. If IsCallable(callbackfn) is false, throw a TypeError exception.
    if (typeof callbackfn !== 'function') {
      throw new TypeError();
    }

    // 5. If thisArg was supplied, let T be thisArg; else let T be undefined.
    if (arguments.length > 1) {
      T = thisArg;
    }

    // 6. Let k be 0.
    k = 0;

    // 7. Repeat, while k < len
    while (k < len) {

      var kValue;

      // a. Let Pk be ToString(k).
      //   This is implicit for LHS operands of the in operator
      // b. Let kPresent be the result of calling the HasProperty internal 
      //    method of O with argument Pk.
      //   This step can be combined with c
      // c. If kPresent is true, then
      if (k in O) {

        // i. Let kValue be the result of calling the Get internal method
        //    of O with argument Pk.
        kValue = O[k];

        // ii. Let testResult be the result of calling the Call internal method
        //     of callbackfn with T as the this value and argument list 
        //     containing kValue, k, and O.
        var testResult = callbackfn.call(T, kValue, k, O);

        // iii. If ToBoolean(testResult) is false, return false.
        if (!testResult) {
          return false;
        }
      }
      k++;
    }
    return true;
  };
}

//--------------------------------------------------------------------------------------------------
// Add size method to Object
//--------------------------------------------------------------------------------------------------
Object.size = function(obj) {
    var size = 0, key;
    for (key in obj) {
        if (obj.hasOwnProperty(key)) size++;
    }
    return size;
};

//--------------------------------------------------------------------------------------------------
// Add join method to Object
//--------------------------------------------------------------------------------------------------
Object.prototype.join = function(sep) {
    var r = [];
    for (var key in this) {
        if (this.hasOwnProperty(key)) {
          r.push(key);
          r.push(this[key]);
        }
    }
    return r.join(sep);
};

// Run the script
try {
    main(parseCommandLineOptions());
}
catch(err) {
    handleScriptError("ELI35991", "Unhandled Error", err);
    WScript.Quit(1);
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
        throw err;
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
// Read all lines from the file (enables reading more from binary files since readAllText will stop
// at EOF chars
//--------------------------------------------------------------------------------------------------
function readLines(fname) {
    // Open the file
    try {
        var f = fso.OpenTextFile(fname, 1);
    }
    catch(err) {
        handleScriptError("ELI42118", "Unable to open input file!", err, "FileName", fname);
    }
    try {
        var ret = [];
        // Read from the file
        while (!f.AtEndOfStream)
            ret.push(f.ReadLine());
        return ret;
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
function getFiles(dirname, recursive, ignoreHiddenDirs) {
  var ret = [];

  _getFiles(dirname);

  function _getFiles(dirname) {
    var folder = fso.GetFolder(dirname);
    if (ignoreHiddenDirs && (folder.Attributes & 2)) {
      return;
    }
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
function getTempFilePath() {
  var TemporaryFolder = 2;
  return fso.BuildPath(fso.GetSpecialFolder(TemporaryFolder).Path, fso.GetTempName());
}

//--------------------------------------------------------------------------------------------------
// Read a CSV file
// Assume well-formed CSV: comma-delimited with all quotes (") escaped by doubling ("")
// and any field with a delim (,) quoted
//--------------------------------------------------------------------------------------------------
function readCSV(path, hasHeader) {
  var lines = readAllText(path).split(/\n/).map(function(line) {
    var fields = line.trim().split(',');
    var dangling = false;
    for (var j=fields.length-1; j >= 0; j--) {
      var field = fields[j];
      var substrings = field.split('"');

      // Even number of split results means odd number of quote chars
      if (substrings.length % 2 == 0) {
        // Add next field to this one and remove next field
        if (dangling) {
          fields[j] += ("," + fields[j+1]);
          fields.splice(j+1, 1)
          dangling = false
        }
        else {
          dangling = true
        }
      }
    }
    return fields;
  });

  if (hasHeader) {
    var headerFields = lines[0];
    lines.splice(0, 1);
    return lines.map(function(fields) {
      var record = {};
      for (var i=0; i < headerFields.length; i++) {
        record[i] = fields[i];
        record[headerFields[i]] = fields[i];
      }
      handleDebug("record", record.join("|"));
      return record;
    });
  }

  return lines;
}

//--------------------------------------------------------------------------------------------------
