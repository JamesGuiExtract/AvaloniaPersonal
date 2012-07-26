//--------------------------------------------------------------------------------------------------
// Script commands specific for ParseWabashaTriminIndexData
//--------------------------------------------------------------------------------------------------
//
//--------------------------------------------------------------------------------------------------
// Script Description
//--------------------------------------------------------------------------------------------------
// Creates Legal Description or PIN EAVs from the index data in the file named by the first argument
// and outputs the results to files below the directory named by the second argument.
//
// Usage:   ParseWabashaTriminIndexData inputFile outputDir csvType
//              inputFile - The path to the index data file
//              outputDir - The path to the EAV output dir
//
// Example inputFile:
//--------------------------------------------------------------------------------------------------

function main(args) {
    var inputFile = fso.getAbsolutePathName(args[0]);
    var outputDir = fso.getAbsolutePathName(args[1]);
    var imageDir = fso.BuildPath(fso.getParentFolderName(outputDir), "Images");

    var csvRegex = /(?:(?:^|,)(\s*"(?:[^"]|"")*"|[^,]*))(?=$|[\r\n,])/g;

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
            fields.push(val.trim());
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
            handleScriptError("ParseWabashaTriminIndexData_3", "Unable to open output file!", err, "FileName", fname);
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
        var name = partialName+".tif.eav";
        return fso.BuildPath(eavdir, name);;
    }

    //--------------------------------------------------------------------------------------------------
    // Specific type functions
    //--------------------------------------------------------------------------------------------------
    function makeEAVS_LEGAL(fields) {
        var fnamefield = fields[1];
        var fname = getEAVName(fnamefield, "LegalDescription");
        var val = "n.a."
        writeAttr(fname, "LegalDescription", val, "", "");
        try {
            val = fields[10];
            if (val != "") {
                writeAttr(fname, "Block", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseWabashaTriminIndexData_4", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[16];
            if (val != "") {
                writeAttr(fname, "Building", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseWabashaTriminIndexData_5", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[17];
            if (val != "") {
                writeAttr(fname, "Garage", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseWabashaTriminIndexData_6", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[9]
            if (val != "") {
                writeAttr(fname, "Lot", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseWabashaTriminIndexData_7", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[11];
            if (val != "") {
                writeAttr(fname, "Q", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseWabashaTriminIndexData_8", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[12];
            if (val != "") {
                writeAttr(fname, "QQ", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseWabashaTriminIndexData_9", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[5];
            if (val != "" && val != "0") {
                writeAttr(fname, "Range", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseWabashaTriminIndexData_10", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[3];
            if (val != "" && val != "0") {
                writeAttr(fname, "Section", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseWabashaTriminIndexData_11", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[18];
            if (val != "") {
                writeAttr(fname, "Subdivision", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseWabashaTriminIndexData_12", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[4];
            if (val != "" && val != "0") {
                writeAttr(fname, "Township", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseWabashaTriminIndexData_13", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[15];
            if (val != "") {
                writeAttr(fname, "Unit", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseWabashaTriminIndexData_14", "Error!", err, "Index Data Line", fields);
        }
    }
}
