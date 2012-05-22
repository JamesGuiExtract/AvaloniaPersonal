//--------------------------------------------------------------------------------------------------
// Script commands specific for ParseRacineTriminIndexData
//--------------------------------------------------------------------------------------------------
//
//--------------------------------------------------------------------------------------------------
// Script Description
//--------------------------------------------------------------------------------------------------
// Creates Legal Description or PIN EAVs from the index data in the file named by the first argument
// and outputs the results to files below the directory named by the second argument.
//
// For Legal Description:
//   Assumes "PlatFile.csv" is in same dir as inputFile and is a mapping of plat code numbers to
//   subdivision names.
//
// For PIN:
//   Assumes "ParcelIDFile.csv" is in same dir as inputFile and is a mapping of plat code numbers to
//   subdivision names.
//
// Usage:   ParseRacineTriminIndexData inputFile outputDir csvType
//              inputFile - The path to the index data file
//              outputDir - The path to the EAV output dir
//              csvType - pin, or legals
//
// Example inputFile:
// K:\Common\Engineering\Sample Files\Trimin\WI - Racine\Set001\IndexData\OriginalsFromCustomer\legal00002.csv
// K:\Common\Engineering\Sample Files\Trimin\WI - Racine\Set002\IndexData\LEGAL00002.csv
//--------------------------------------------------------------------------------------------------

function main(args) {
    var inputFile = fso.getAbsolutePathName(args[0]);
    var outputDir = fso.getAbsolutePathName(args[1]);
    var imageDir = fso.BuildPath(fso.getParentFolderName(outputDir), "Images");

    var csvRegex = /(?:(?:^|,)(\s*"(?:[^"]|"")*"|[^,]*))(?=$|[\r\n,])/g;

    switch(args[2].toUpperCase()) {
          case "PIN": csvType = "pin";
          break;
          case "LEGALS": csvType = "legals";
          break;
          default: return;
    }

    if (csvType == "pin") {
      // populate pin code to name map
      var pinnames = readAllText(fso.BuildPath(fso.getParentFolderName(inputFile), "ParcelIDFile.csv")).split(/\n/);
      var pinmap = {};
      for (var i=0; i < pinnames.length; i++) {
          var fields = [];
          var match = csvRegex.exec(pinnames[i]);
          while (match != null) {
              var val = match[1];
              var quoted = val.match(/^\s*"((?:""|[^"])*)"$/);
              if (quoted != null) {
                  val = quoted[1].replace(/""/g, "\"");
              }
              fields.push(val);
              match = csvRegex.exec(pinnames[i]);
          }
          pinmap[fields[0]] = fields[1];
      }

    } else if (csvType == "legals") {
      // populate subd code to name map
      var subdnames = readAllText(fso.BuildPath(fso.getParentFolderName(inputFile), "PlatFile.csv")).split(/\n/);
      var subdmap = {};
      for (var i=0; i < subdnames.length; i++) {
          var fields = [];
          var match = csvRegex.exec(subdnames[i]);
          while (match != null) {
              var val = match[1];
              var quoted = val.match(/^\s*"((?:""|[^"])*)"$/);
              if (quoted != null) {
                  val = quoted[1].replace(/""/g, "\"");
              }
              fields.push(val);
              match = csvRegex.exec(subdnames[i]);
          }
          subdmap[fields[1]] = fields[2];
      }
    }

    var csvrecords = [];
    // Split on lines
    var csvrecords = readAllText(inputFile).split(/\n/).slice(1);

    if (!fso.folderExists(outputDir)) {
        fso.createFolder(outputDir);
    }

    for (var i=0; i < csvrecords.length; i++) {
        handleDebug("CSVLine", i);
        var fields = [];
        var match = csvRegex.exec(csvrecords[i]);
        while (match != null) {
            var val = match[1];
            var quoted = val.match(/^\s*"((?:""|[^"])*)"$/);
            if (quoted != null) {
                val = quoted[1].replace(/""/g, "\"");
            }
            fields.push(val);
            match = csvRegex.exec(csvrecords[i]);
        }

        if (csvType == "pin") {
          makeEAVS_PIN(fields);
        } else if (csvType == "legals") {
          makeEAVS_LEGAL(fields);
        }
    }

    function appendText(fname, text) {
        // Open the file
        try {
            var f = fso.OpenTextFile(fname, 8, true);
        }
        catch(err) {
            handleScriptError("ParseRacineTriminIndexData_3", "Unable to open output file!", err, "FileName", fname);
        }
        // Write to the file
        f.Write(text);
        f.Close();
    }

    //--------------------------------------------------------------------------------------------------
    // Generic attribute writer
    //--------------------------------------------------------------------------------------------------
    function writeAttr(fname, aname, avalue, atype, indent) {
        if (avalue!=undefined && avalue!="" && !(/^0+$/.test(avalue))) {
            if (fso.fileExists(fname)) {
                indent = "\n"+indent;
            }
            appendText(fname, indent+aname+"|"+avalue+(atype? "|"+atype : ""));
        }
    }

    function getEAVName(partialName, aname) {
        var eavdir = fso.BuildPath(outputDir, aname);
        if (!fso.folderExists(eavdir)) {
            fso.createFolder(eavdir);
        }
        var name = partialName+"M001.tif.eav";
        return fso.BuildPath(eavdir, name);;
    }

    //--------------------------------------------------------------------------------------------------
    // Specific type functions
    //--------------------------------------------------------------------------------------------------
    function makeEAVS_LEGAL(fields) {
        var fnamefield;
        if (fields.length > 28) {
          fnamefield = fields[29]
        } else {
          fnamefield = fields[26]
        }
        var fname = getEAVName(fnamefield, "LegalDescription");
        var val = "n.a."
        writeAttr(fname, "LegalDescription", val, "", "");
        try {
            val = fields[16];
            if (val != "") {
                writeAttr(fname, "Block", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseRacineTriminIndexData_4", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[19];
            if (val != "") {
                writeAttr(fname, "Building", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseRacineTriminIndexData_5", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[20];
            if (val != "") {
                writeAttr(fname, "Garage", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseRacineTriminIndexData_6", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[8];
            if (val != "") {
                writeAttr(fname, "Lot", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseRacineTriminIndexData_7", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[10];
            if (val != "") {
                writeAttr(fname, "Q", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseRacineTriminIndexData_8", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[11];
            if (val != "") {
                writeAttr(fname, "QQ", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseRacineTriminIndexData_9", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[6];
            if (val != "") {
                writeAttr(fname, "Range", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseRacineTriminIndexData_10", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[4];
            if (val != "") {
                writeAttr(fname, "Section", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseRacineTriminIndexData_11", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[15];
            if (val != "") {
                writeAttr(fname, "Subdivision", subdmap[val], "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseRacineTriminIndexData_12", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[5];
            if (val != "") {
                writeAttr(fname, "Township", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseRacineTriminIndexData_13", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[18];
            if (val != "") {
                writeAttr(fname, "Unit", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseRacineTriminIndexData_14", "Error!", err, "Index Data Line", fields);
        }
    }

    function makeEAVS_PIN(fields) {
        var fname = getEAVName(fields[26], "PIN");
        var val
        try {
            val = fields[15];
            if (val != "") {
                writeAttr(fname, "PIN", pinmap[val], "", "");
            }
        }
        catch(err) {
            handleScriptError("ParseRacineTriminIndexData_15", "Error!", err, "Index Data Line", fields);
        }
    }

}
