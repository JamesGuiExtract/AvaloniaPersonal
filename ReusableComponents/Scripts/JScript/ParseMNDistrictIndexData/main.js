//--------------------------------------------------------------------------------------------------
// Script commands specific for ParseMNDistrictIndexData
//--------------------------------------------------------------------------------------------------
//
//--------------------------------------------------------------------------------------------------
// Script Description
//--------------------------------------------------------------------------------------------------
// Creates EAVs out of the index data in the file named by the first argument and outputs
// the results to files below the directory named by the second argument.
//
// Assumes images are in Images dir parallel to output dir
//
// Usage:   ParseMNDistrictIndexData inputFile outputDir
//              inputFile - The path to the index data file
//              outputDir - The path to the EAV output dir
//              csvType - docs, legals, parties, or related
//
// Example inputFiles:
//  k:\Common\Engineering\Sample Files\Tyler\MN - District\Carlton\IndexData\
//  
//  
//--------------------------------------------------------------------------------------------------

function main(args) {
    var inputFile = fso.getAbsolutePathName(args[0]);
    var outputDir = fso.getAbsolutePathName(args[1]);
    var imageDir = fso.BuildPath(fso.getParentFolderName(outputDir), "Images");
    var csvType = "";
    switch(args[2].toUpperCase()) {
          case "DOCS": csvType = "docs";
          break;
          case "LEGALS": csvType = "legals";
          break;
          case "PARTIES": csvType = "parties";
          break;
          case "RELATED": csvType = "related";
          break;
          default: return;
    }

    var csvrecords = [];
    var isInMultiline = false;
    // Split on lines
    var csvlines = readAllText(inputFile).split(/\n/);

    // Fix errors in CSV syntax (unescaped double quotes)
    // This will fix some anyway...
    csvlines = csvlines.map(function(l){return l.replace(/([^,\s]\s*")(?!\s*(?:,|$))(?=(?:"")*[^"])/g, "$1\"")});

    // Merge multi-line records
    for (var i=0; i < csvlines.length; i++) {
        var line = csvlines[i].trim();
        var count = 0;  
        var pos = line.indexOf('"');  
        while ( pos != -1 ) {  
           count++;  
           pos = line.indexOf('"',pos+1);  
        }
        var isPartRecord = (count % 2 == 1);
        if (isInMultiline) { // The previous line was part of a multi-line record
            if (isPartRecord) { // This must be the last part of the record
                var record = csvrecords[csvrecords.length-1] + "\\r\\n" + line;
                csvrecords[csvrecords.length-1] = record;
                isInMultiline = false;
            } else { // This is an intermediate line in the record
                var record = csvrecords[csvrecords.length-1] + "\\r\\n" + line;
                csvrecords[csvrecords.length-1] = record;
            }
        } else { // The previous line was either a whole record, or the last part of a record
            if (isPartRecord) { // This is the start of a multi-line record
                isInMultiline = true;
                csvrecords.push(line);
            } else { // This is a single-line record
                csvrecords.push(line);
            }
        }
    }

    if (csvType == "docs") {
        var imageFiles = getFiles(imageDir, true).filter(function(f){return f.Name.match(/\.(tiff?|pdf|\d{3})$/i)});
        var imageMap = {};
        for (var i=0; i < imageFiles.length; i++) {
            var f = imageFiles[i];
            imageMap[f.Name.replace(/\..*/,"")] = f;
        }

        // Create a map of images to destinations
        // This is to deal with images that have multiple records and multiple doc-types
        var imagesToCopy = {};
    }

    if (!fso.folderExists(outputDir)) {
        fso.createFolder(outputDir);
    }

    for (var i=0; i < csvrecords.length; i++) {
        handleDebug("CSVLine", i);
        var fields = [];
        var csvRegex = /(?:(?:^|,)(\s*"(?:[^"]|"")*"|[^,]*))(?=$|,)/g;
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

        switch(csvType) {
            case "docs":
                // Update map of images to copy
                setCopyImage(fields);
                makeEAVS_DOCS(fields);
            break;
            case "legals":
                makeEAVS_LEGAL(fields);
            break;
            case "parties":
                makeEAVS_PARTIES(fields);
            break;
            case "related":
                handleDebug("Fields", fields);
                makeEAVS_REFERENCE(fields);
            break;
            default: return;
        }
    }
    
    if (csvType == "docs") {
        copyImages(imagesToCopy);
    }

    // Update map of images to destinations based on doc-type(s)
    function setCopyImage(fields) {
        var doctype = fields[3].replace(/[^\w\s]/g, "_").replace(/_+$/,"");
        var iname = getImageName(fields[0]);
        if (iname == undefined) {
            return;
        }
        var existRecord = imagesToCopy[iname];
        if (existRecord) {
            if (existRecord.has(doctype)) {
                return;
            } else {
                existRecord.push(doctype);
            }
        } else {
            imagesToCopy[iname] = [doctype];
        }
    }

    function copyImages(imagesToDocTypes) {
        for (iname in imagesToDocTypes) {
            var doctypes = imagesToDocTypes[iname];
            var doctype = doctypes.join(" and ");
            var newdir = fso.BuildPath(imageDir, doctype);

            try {
                if (!fso.folderExists(newdir)) {
                    fso.createFolder(newdir);
                }
            }
            catch(err) {
                handleScriptError("ParseMNDistrictIndexData_1", "Unable to create folder!", err, "FolderName", newdir);
            }

            try {
                fso.CopyFile(iname, newdir+"\\");
            }
            catch(err) {
                handleScriptError("ParseMNDistrictIndexData_2", "Unable to copy file!", err, "FileName", iname, "Destination", newdir);
            }
        }
    }

    function appendText(fname, text) {
        // Open the file
        try {
            var f = fso.OpenTextFile(fname, 8, true);
        }
        catch(err) {
            handleScriptError("ParseMNDistrictIndexData_3", "Unable to open output file!", err, "FileName", fname);
        }
        // Write to the file
        f.Write(text);
        f.Close();
    }

    //--------------------------------------------------------------------------------------------------
    // Generic attribute writer
    //--------------------------------------------------------------------------------------------------
    function writeAttr(fname, aname, avalue, atype, indent) {
        if (avalue!=undefined && avalue!="" && avalue!="NULL" && !(/^0+$/.test(avalue))) {
            if (fso.fileExists(fname)) {
                indent = "\n"+indent;
            }
            appendText(fname, indent+aname+"|"+avalue+(atype? "|"+atype : ""));
        }
    }

    function getImageName(partialName) {
        var file = imageMap[partialName];
        if (file == undefined) {
            handleScriptError("ParseMNDistrictIndexData_4", "Can't figure out file name!",
                {number: 1234, description: partialName});
        } else {
          return file.Path;
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
    function makeEAVS_DOCS(fields) {
        var fname = getEAVName(fields[0], "DocumentDate");
        try {
            var val = fields[2].trim();
            writeAttr(fname, "DocumentDate", val, "", "");
        }
        catch(err) {
            handleScriptError("ParseMNDistrictIndexData_6", "Error!", err, "Index Data Line", fields);
        }
        fname = getEAVName(fields[0], "ConsiderationAmount");
        try {
            val = fields[5].trim();
            writeAttr(fname, "ConsiderationAmount", val, "", "");
        }
        catch(err) {
            handleScriptError("ParseMNDistrictIndexData_7", "Error!", err, "Index Data Line", fields);
        }
    }

    function makeEAVS_REFERENCE(fields) {
        var fname = getEAVName(fields[0], "RelatedNumber");
        var val = "N/A"
        writeAttr(fname, "Reference", val, "", "");
        try {
            val = fields[2];
            writeAttr(fname, "Book", val, "", ".");
        }
        catch(err) {
            handleScriptError("ParseMNDistrictIndexData_8", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[1];
            writeAttr(fname, "Instrument", val, "", ".");
        }
        catch(err) {
            handleScriptError("ParseMNDistrictIndexData_9", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[3];
            writeAttr(fname, "Page", val, "", ".");
        }
        catch(err) {
            handleScriptError("ParseMNDistrictIndexData_10", "Error!", err, "Index Data Line", fields);
        }
    }

    function makeEAVS_PARTIES(fields) {
        var typ = fields[2];
        var fname = getEAVName(fields[0], typ);
        try {
            var val = fields[1].trim();
            writeAttr(fname, typ, val, "", "");
        }
        catch(err) {
            handleScriptError("ParseMNDistrictIndexData_11", "Error!", err, "Index Data Line", fields);
        }
    }

    function makeEAVS_LEGAL(fields) {
        var fname = getEAVName(fields[0], "LegalDescription");
        var val = "N/A"
        writeAttr(fname, "LegalDescription", val, "", "");
        try {
            val = fields[1];
            if (val != "") {
                writeAttr(fname, "Municipality", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseMNDistrictIndexData_12", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[2];
            if (val != "") {
                writeAttr(fname, "Subdivision", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseMNDistrictIndexData_13", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[3];
            if (val != "") {
                writeAttr(fname, "Block", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseMNDistrictIndexData_14", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[4];
            if (val != "") {
                writeAttr(fname, "Lot", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseMNDistrictIndexData_15", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[6];
            if (val != "") {
                writeAttr(fname, "Range", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseMNDistrictIndexData_16", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[7];
            if (val != "") {
                writeAttr(fname, "Section", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseMNDistrictIndexData_17", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[5];
            if (val != "") {
                writeAttr(fname, "Township", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseMNDistrictIndexData_18", "Error!", err, "Index Data Line", fields);
        }
        try {
            val = fields[8];
            if (val != "") {
                writeAttr(fname, "Tract", val, "", ".");
            }
        }
        catch(err) {
            handleScriptError("ParseMNDistrictIndexData_19", "Error!", err, "Index Data Line", fields);
        }
    }

}
