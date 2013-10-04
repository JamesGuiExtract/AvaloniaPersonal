//--------------------------------------------------------------------------------------------------
// Script commands specific for ParseTuscolaACSIndexData
//--------------------------------------------------------------------------------------------------
//
//--------------------------------------------------------------------------------------------------
// Script Description
//--------------------------------------------------------------------------------------------------
// Create EAVs out of the mostly CSV index data in the file named by the first argument and outputs
// the results to files below the directory named by the second argument.
//
// Also copies the images to subfolders by document type (assumes images are in Images dir parallel
// to output dir)
//
// Usage:   ParseTuscolaACSIndexData inputFile outputDir
//              inputFile - The path to the index data file
//              outputDir - The path to the EAV output dir
// 
// Example input file: K:\Common\Engineering\Sample Files\ACS\MI - Tuscola\Set004\IndexData\TUSCOLA_MI_INDEX.TXT
//--------------------------------------------------------------------------------------------------

function main(args) {
    var inputFile = fso.getAbsolutePathName(args[0]);
    var outputDir = fso.getAbsolutePathName(args[1]);
    var imageDir = fso.BuildPath(fso.getParentFolderName(outputDir), "Images");

    handleDebug("ImageDir", imageDir);
    var imageFiles = getFiles(imageDir, true).filter(function(f){return f.Name.match(/\.(tiff?|pdf|\d{3})$/i)});
    var imageMap = {};
    for (var i=0; i < imageFiles.length; i++) {
        handleDebug("Loop", i);
        var f = imageFiles[i];
        imageMap[f.Name.slice(1,10)] = f;
    }

    if (!fso.folderExists(outputDir)) {
        fso.createFolder(outputDir);
    }

    var csvlines = readAllText(inputFile).split(/\n/).map(function(s){return s.trim()});
    for (var i=0; i < csvlines.length; i++) {
        handleDebug("CSVLine", i);
        var fields = csvlines[i].split(',').map(function(s){return s.trim()});
        switch(fields[1]){
            case "DOCUMENT": makeEAVS_DOCUMENT(fields);
            break;
            case "MAILBACK": makeEAVS_MAILBACK(fields);
            break;
            case "PARTIES": makeEAVS_PARTIES(fields);
            break;
            case "PROPERTY": makeEAVS_PROPERTY(fields);
            break;
            case "REFERENCE": makeEAVS_REFERENCE(fields);
            break;
        }
    }

    function appendText(fname, text) {
        // Open the file
        var isOpen = false;
        var attempts = 0;

        // Try to open file up to 50 times before exception
        try {
            while(isOpen == false) {
                attempts++;
                try {
                    var f = fso.OpenTextFile(fname, 8, true);
                    isOpen = true;
                }
                catch(err) {
                    if (attempts == 50) { throw err; }
                    WScript.Sleep(100);
                }
            }
        }
        catch(err) { handleScriptError("ParseTuscolaACSIndexData_2", "Unable to open output file!", err, "FileName", fname); }
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
                indent = "\r\n"+indent;
            }
            appendText(fname, indent+aname+"|"+avalue+(atype? "|"+atype : ""));
        }
    }

    function getImageName(partialName) {
        var file = imageMap[partialName];
        if (file == undefined) {
            handleScriptError("ParseTuscolaACSIndexData_3", "Can't figure out file name!",
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
        var iname = getImageName(partialName);
        return fso.BuildPath(eavdir, fso.GetFileName(iname)+".eav");;
    }

    //--------------------------------------------------------------------------------------------------
    // Specific type functions
    //--------------------------------------------------------------------------------------------------
    function makeEAVS_DOCUMENT(fields) {
        var fname = getEAVName(fields[0], "Document");
        writeAttr(fname, "DocumentNumber", fields[2], "", "");
        writeAttr(fname, "RecordedDate", fields[5], "", "");
        writeAttr(fname, "RecordedTime", fields[6], "", "");
        writeAttr(fname, "BookCode", fields[8], "", "");
        writeAttr(fname, "Book", fields[9], "", "");
        writeAttr(fname, "Page", fields[10], "", "");
        writeAttr(fname, "Consideration", fields[13], "", "");
        writeAttr(fname, "RecordingFee", fields[17], "", "");
        writeAttr(fname, "NoteAmount", fields[19], "", "");

        // Copy the image to a subfolder
        var doctype = fields[4];
        var iname = getImageName(fields[0]);
        if (iname == undefined) {
            return;
        }
        var newdir = fso.BuildPath(fso.GetParentFolderName(iname), doctype.replace(/[^\w\s]/g, "_").replace(/_+$/,""));

        if (fso.FileExists(fso.BuildPath(newdir, fso.GetFileName(iname)))) {
            return;
        }
        try {
            if (!fso.folderExists(newdir)) {
                fso.createFolder(newdir);
            }
        }
        catch(err) {
            handleScriptError("ParseTuscolaACSIndexData_4", "Unable to create folder!", err, "FolderName", newdir);
        }

        try {
            fso.CopyFile(iname, newdir+"\\");
        }
        catch(err) {
            handleScriptError("ParseTuscolaACSIndexData_5", "Unable to copy file!", err, "FileName", iname, "Destination", newdir);
        }
    }
    function makeEAVS_MAILBACK(fields) {
        var fname = getEAVName(fields[0], "ReturnAddress");
        writeAttr(fname, "ReturnAddress", "N/A", "", "");
        writeAttr(fname, "Recipient1", fields[2], "", ".");
        writeAttr(fname, "Address1", fields[3], "", ".");
        writeAttr(fname, "Address2", fields[4], "", ".");
        writeAttr(fname, "City", fields[5], "", ".");
        writeAttr(fname, "State", fields[6], "", ".");
        writeAttr(fname, "ZipCode", fields[7], "", ".");
    }
    function makeEAVS_PARTIES(fields) {
        var typ = "";
        switch(fields[2]) {
            case "D": typ = "Grantor";
            break;
            case "I": typ = "Grantee";
            break;
            case "T": typ = "Interested";
            break;
            default: typ = "Unknown";
        }
        var fname = getEAVName(fields[0], typ);
        writeAttr(fname, typ, fields.slice(3).join(" ").trim(), "", "");
    }
    function makeEAVS_PROPERTY(fields) {
        var fname = getEAVName(fields[0], "LegalDescription");

        // Resplit because I've noticed commas in the data
        var data = fields.slice(3).join(",").split(/,(?=[A-Z]*-|,*$)/i);
        switch(fields[2]) {
            case "SUBDIVISIONS":
                writeAttr(fname, "LegalDescription", "N/A", "Subdivision", "");
                for (var i=0; i<data.length; i++) {
                    var parts = data[i].split('-');
                    var key = parts[0];
                    var value = parts.slice(1).join("-");
                    switch(key) {
                        case "NM":
                            writeAttr(fname, "Subdivision", value.replace(/\s*[(_].*/, ""), "", ".");
                            break;
                        case "BLOCK":
                            writeAttr(fname, "Block", value, "", ".");
                            break;
                        case "FLOT":
                            writeAttr(fname, "FromLot", value, "", ".");
                            break;
                        case "TLOT":
                            writeAttr(fname, "ToLot", value, "", ".");
                            break;
                        case "PIN":
                            writeAttr(fname, "PIN", value, "", ".");
                            break;
                        default: handleScriptError("ParseTuscolaACSIndexData_6", "Unknown subtype!", {number: 1234, description: "Unknown sub-typ, '"+key+"'"}, "EAV", fname);
                    }
                }
                break;
            case "SECTION LAND":
                writeAttr(fname, "LegalDescription", "N/A", "Section", "");
                for (var i=0; i<data.length; i++) {
                    var parts = data[i].split('-');
                    var key = parts[0];
                    var value = parts.slice(1).join("-");
                    switch(key) {
                        case "SECTION":
                            writeAttr(fname, "Section", value, "", ".");
                            break;
                        case "TOWN":
                            writeAttr(fname, "Township", value, "", ".");
                            break;
                        case "RANGE":
                            writeAttr(fname, "Range", value, "", ".");
                            break;
                        case "QUARTERS":
                            writeAttr(fname, "Quarters", value, "", ".");
                            break;
                        case "HALF":
                            writeAttr(fname, "Half", value, "", ".");
                            break;
                        case "PIN":
                            writeAttr(fname, "PIN", value, "", ".");
                            break;
                        case "ACRES":
                            writeAttr(fname, "Acres", value, "", ".");
                            break;
                        default: handleScriptError("ParseTuscolaACSIndexData_7", "Unknown subtype!", {number: 1234, description: "Unknown sub-typ, '"+key+"'"}, "EAV", fname);
                    }
                }
                break;
            case "CONDOMINIUMS":
                writeAttr(fname, "LegalDescription", "N/A", "Condominium", "");
                for (var i=0; i<data.length; i++) {
                    var parts = data[i].split('-');
                    var key = parts[0];
                    var value = parts.slice(1).join("-");
                    switch(key) {
                        case "NM":
                            writeAttr(fname, "Condominium", value.replace(/\s*[(_].*/, ""), "", ".");
                            break;
                        case "BLDG":
                            writeAttr(fname, "Building", value, "", ".");
                            break;
                        case "UNIT":
                            writeAttr(fname, "Unit", value, "", ".");
                            break;
                        default: handleScriptError("ParseTuscolaACSIndexData_8", "Unknown subtype!", {number: 1234, description: "Unknown sub-typ, '"+key+"'"}, "EAV", fname);
                    }
                }
                break;
        }
    }
    function makeEAVS_REFERENCE(fields) {
        var fname = getEAVName(fields[0], "Reference");
        switch(fields[2]) {
            case "1":
                writeAttr(fname, "Year", fields[3], "", "");
                writeAttr(fname, "Book", fields[4], "", "");
                writeAttr(fname, "Volume", fields[5], "", "");
                writeAttr(fname, "Page", fields[6], "", "");
                break;
            case "2":
                writeAttr(fname, "Date", fields[3], "", "");
                writeAttr(fname, "DocumentNumber", fields[4], "", "");
                break;
        }
    }
}
