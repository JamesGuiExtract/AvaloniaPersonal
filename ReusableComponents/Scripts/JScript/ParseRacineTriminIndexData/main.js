//--------------------------------------------------------------------------------------------------
// Script commands specific for ParseRacineTriminIndexData
//--------------------------------------------------------------------------------------------------
//
//--------------------------------------------------------------------------------------------------
// Script Description
//--------------------------------------------------------------------------------------------------
// Creates Legal Description EAVs out of the index data in the file named by the first argument and
// outputs the results to files below the directory named by the second argument.
//
// Assumes "PlatFile.csv" is in same dir as inputFile and is a mapping of plat code numbers to
// subdivision names.
//
// Usage:   ParseRacineTriminIndexData inputFile outputDir
//              inputFile - The path to the index data file
//              outputDir - The path to the EAV output dir
//
// Example inputFile:
// K:\Common\Engineering\Sample Files\Trimin\WI - Racine\Set001\IndexData\OriginalsFromCustomer\legal00002.csv
//--------------------------------------------------------------------------------------------------

function main(args) {
    var inputFile = fso.getAbsolutePathName(args[0]);
    var outputDir = fso.getAbsolutePathName(args[1]);
    var imageDir = fso.BuildPath(fso.getParentFolderName(outputDir), "Images");

    var csvRegex = /(?:(?:^|,)(\s*"(?:[^"]|"")*"|[^,]*))(?=$|[\r\n,])/g;

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

        makeEAVS_LEGAL(fields);
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
        var fname = getEAVName(fields[26], "LegalDescription");
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

}
