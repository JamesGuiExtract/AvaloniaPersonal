//--------------------------------------------------------------------------------------------------
// Script commands specific for ParseSiskiyouAtPacIndexData
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
// Usage:   ParseSiskiyouAtPacIndexData inputFile outputDir
//              inputFile - The path to the index data file
//              outputDir - The path to the EAV output dir
//
// Example inputFile: K:\Common\Engineering\Sample Files\AtPac\CA - Siskiyou\Set001\IndexData\sis_extract.txt
//--------------------------------------------------------------------------------------------------

function main(args) {
    var inputFile = fso.getAbsolutePathName(args[0]);
    var outputDir = fso.getAbsolutePathName(args[1]);
    var imageDir = fso.BuildPath(fso.getParentFolderName(outputDir), "Images");

    var imageFiles = getFiles(imageDir, true).filter(function(f){return f.Name.match(/\.(tiff?|pdf|\d{3})$/i)});
    var imageMap = {};
    for (var i=0; i < imageFiles.length; i++) {
        var f = imageFiles[i];
        imageMap[f.Name.slice(0,4)+f.Name.slice(5,12)] = f;
    }

    if (!fso.folderExists(outputDir)) {
        fso.createFolder(outputDir);
    }

    // Create a map of images to destinations
    // This is to deal with images that have multiple records and multiple doc-types
    var imagesToCopy = {};

    var csvlines = readAllText(inputFile).split(/\n/).map(function(s){return s.trim()});
    for (var i=0; i < csvlines.length; i++) {
        handleDebug("CSVLine", i);
        var fields = csvlines[i].split(/\s*,\s*/);

        // Update map of images to copy
        setCopyImage(fields);

        if (fields.length > 5) {
            if (fields[5].match(/^(?:(?:REF[#]?|EF|F|RE|RERF|RF)\s?)?[-\x20\d]{5}/i)) {
                makeEAVS_REFERENCE(fields);
            } else {
                makeEAVS_PARTIES(fields);
            }
        } else {
            makeEAVS_PIN(fields);
        }
    }
    
    copyImages(imagesToCopy);

    // Update map of images to destinations based on doc-type(s)
    function setCopyImage(fields) {
        var doctype = fields[3].replace(/[^\w\s]/g, "_").replace(/_+$/,"");
        var iname = getImageName(fields[0]+fields[1].replace(/-\d{2}$/,""));
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
            var newdir = fso.BuildPath(fso.GetParentFolderName(iname), doctype);

            try {
                if (!fso.folderExists(newdir)) {
                    fso.createFolder(newdir);
                }
            }
            catch(err) {
                handleScriptError("ParseSiskiyouAtPacIndexData_1", "Unable to create folder!", err, "FolderName", newdir);
            }

            try {
                fso.CopyFile(iname, newdir+"\\");
            }
            catch(err) {
                handleScriptError("ParseSiskiyouAtPacIndexData_2", "Unable to copy file!", err, "FileName", iname, "Destination", newdir);
            }
        }
    }

    function appendText(fname, text) {
        // Open the file
        try {
            var f = fso.OpenTextFile(fname, 8, true);
        }
        catch(err) {
            handleScriptError("ParseSiskiyouAtPacIndexData_3", "Unable to open output file!", err, "FileName", fname);
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

    function getImageName(partialName) {
        var file = imageMap[partialName];
        if (file == undefined) {
            handleScriptError("ParseSiskiyouAtPacIndexData_4", "Can't figure out file name!",
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
    function makeEAVS_PIN(fields) {
        var attrType = parseInt(fields[1].slice(8,10));
        var letters = "AABCDEFGHIJKLMNOPQRSTUVWXYZ";
        attrType = letters.charAt(attrType);
        var fname = getEAVName(fields[0]+fields[1].replace(/-\d{2}$/,""), "PIN");
        if (fields[4].trim() != '-') {
            try {
                writeAttr(fname, "PIN", fields[4].trim(), attrType, "");
            }
            catch(err) {
                handleScriptError("ParseSiskiyouAtPacIndexData_5", "Error!", err, "Index Data Line", fields);
            }
        }
    }

    function makeEAVS_REFERENCE(fields) {
        var attrType = parseInt(fields[1].slice(8,10));
        var letters = "AABCDEFGHIJKLMNOPQRSTUVWXYZ";
        attrType = letters.charAt(attrType);
        var fname = getEAVName(fields[0]+fields[1].replace(/-\d{2}$/,""), "Reference");
        try {
            var val = fields[5].trim().replace(/^(?:REF[#]?|EF|F|RE|RERF|RF)\s?(?=\d)/, "");
            writeAttr(fname, "Reference", val, attrType, "");
        }
        catch(err) {
            handleScriptError("ParseSiskiyouAtPacIndexData_6", "Error!", err, "Index Data Line", fields);
        }
    }

    function makeEAVS_PARTIES(fields) {
        var typ = "";
        var attrType = parseInt(fields[1].slice(8,10));
        var letters = "AABCDEFGHIJKLMNOPQRSTUVWXYZ";
        attrType = letters.charAt(attrType);
        switch(fields[4]) {
            case "R": typ = "Grantor";
            break;
            case "E": typ = "Grantee";
            break;
            default: typ = "Unknown";
        }
        var fname = getEAVName(fields[0]+fields[1].replace(/-\d{2}$/,""), typ);
        try {
            var val = fields[5].trim().replace(/^(?:REF[#]?|EF|F|RE|RERF|RF)\s/, "");
            writeAttr(fname, typ, val, attrType, "");
        }
        catch(err) {
            handleScriptError("ParseSiskiyouAtPacIndexData_7", "Error!", err, "Index Data Line", fields);
        }
    }
}
