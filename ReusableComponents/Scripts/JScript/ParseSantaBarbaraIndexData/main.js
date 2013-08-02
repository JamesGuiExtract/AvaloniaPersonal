//--------------------------------------------------------------------------------------------------
// Script commands specific for ParseSantaBarbaraIndexData
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
// Usage:   ParseSantaBarbaraIndexData inputFile outputDir
//              inputFile - The path to the index data file
//              outputDir - The path to the EAV output dir
//
// Example inputFile: K:\Common\Engineering\Sample Files\DFM\CA - Santa Barbara\Set013\IndexData\IndexData.csv
//--------------------------------------------------------------------------------------------------

function main(args) {
    var inputFile = fso.getAbsolutePathName(args[0]);
    var outputDir = fso.getAbsolutePathName(args[1]);
    var imageDir = fso.BuildPath(fso.getParentFolderName(outputDir), "Images");

    var imageFiles = getFiles(imageDir, true).filter(function(f){return f.Name.match(/\.(tiff?|pdf|\d{3})$/i)});
    var imageMap = {};
    for (var i=0; i < imageFiles.length; i++) {
        var f = imageFiles[i];
        imageMap[f.Name.slice(0,12)] = f;
    }

    if (!fso.folderExists(outputDir)) {
        fso.createFolder(outputDir);
    }

    // Create a map of images to destinations
    // This is to deal with images that have multiple records and multiple doc-types
    var imagesToCopy = {};

    var csvRegex = /(?:(?:^|,)(\s*"(?:[^"]|"")*"|[^,]*))(?=$|[\r\n,])/g;
    var csvlines = readAllText(inputFile).split(/\n/).map(function(s){return s.trim()});
    var records = [];

    var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    // Update images-to-copy map and write doc-type EAVs
    for (var i=0; i < csvlines.length; i++) {
        var fields = [];
        var match = csvRegex.exec(csvlines[i]);
        while (match != null) {
            var val = match[1];
            var quoted = val.match(/^\s*"((?:""|[^"])*)"$/);
            if (quoted != null) {
                val = quoted[1].replace(/""/g, "\"");
            }
            fields.push(val);
            match = csvRegex.exec(csvlines[i]);
        }

        // Update map of images to destinations based on doc-type(s)
        var doctype = fields[1].replace(/[^\w\s]/g, "_").replace(/_+$/,"");
        var docnum = fields[0].match(/^(\d+-\d+)(?:-(\d{3}))?$/);
        if (docnum.length > 2) {
            var title = letters.charAt(parseInt(docnum[2]));
        } else {
            var title = "A";
        }
        var iname = getImageName(docnum[1]);
        fields = [iname,title].concat(fields.slice(1));

        var existRecord = imagesToCopy[iname];
        if (existRecord) {
            if (!existRecord.has(doctype)) {
                existRecord.push(doctype);
                makeEAVS_DOCTYPE(fields);
            }
        } else {
            imagesToCopy[iname] = [doctype];
            makeEAVS_DOCTYPE(fields);
        }

        records.push(fields);
    }

    for (var i=0; i < records.length; i++) {
        var fields = records[i];
        var fieldType = fields[fields.length-1];
        switch(fieldType) {
            case "O": makeEAVS_GRANTOR(fields);
            break;
            case "E": makeEAVS_GRANTEE(fields);
            break;
            case "I": makeEAVS_INDEXITEM(fields);
            break;
            case "A": makeEAVS_PIN(fields);
            break;
            case "R": makeEAVS_REFERENCE(fields);
            break;
        }
    }
    
    copyImages(imagesToCopy);

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
                handleScriptError("ParseSantaBarbaraIndexData_1", "Unable to create folder!", err, "FolderName", newdir);
            }

            try {
                fso.CopyFile(iname, newdir+"\\");
            }
            catch(err) {
                handleScriptError("ParseSantaBarbaraIndexData_2", "Unable to copy file!", err, "FileName", iname, "Destination", newdir);
            }
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
        catch(err) { handleScriptError("ParseSantaBarbaraIndexData_3", "Unable to open output file!", err, "FileName", fname); }
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
            handleScriptError("ParseSantaBarbaraIndexData_4", "Can't figure out file name!",
                {number: 1234, description: partialName});
        } else {
            return file.Path;
        }
    }

    function getEAVName(iname, aname) {
        var eavdir = fso.BuildPath(outputDir, aname);
        if (!fso.folderExists(eavdir)) {
            fso.createFolder(eavdir);
        }
        return fso.BuildPath(eavdir, fso.GetFileName(iname)+".eav");
    }

    //--------------------------------------------------------------------------------------------------
    // Specific type functions
    //--------------------------------------------------------------------------------------------------
    function makeEAVS_PIN(fields) {
        var attrType = fields[1];
        var val = fields[3].trim();
        var fname = getEAVName(fields[0], "APN");
        if (val != '-') {
            try {
                writeAttr(fname, "APN", val, attrType, "");
            }
            catch(err) {
                handleScriptError("ParseSantaBarbaraIndexData_5", "Error!", err, "Index Data", fields);
            }
        }
    }

    function makeEAVS_REFERENCE(fields) {
        var attrType = fields[1];
        var val = fields[3].trim();
        var fname = getEAVName(fields[0], "Reference Field");
        try {
            writeAttr(fname, "ReferenceField", val, attrType, "");
        }
        catch(err) {
            handleScriptError("ParseSantaBarbaraIndexData_6", "Error!", err, "Index Data", fields);
        }
    }

    function makeEAVS_GRANTOR(fields) {
        var attrType = fields[1];
        var val = fields[3].trim();
        var fname = getEAVName(fields[0], "Grantor");
        try {
            writeAttr(fname, "Grantor", val, attrType, "");
        }
        catch(err) {
            handleScriptError("ParseSantaBarbaraIndexData_7", "Error!", err, "Index Data", fields);
        }
    }

    function makeEAVS_GRANTEE(fields) {
        var attrType = fields[1];
        var val = fields[3].trim();
        var fname = getEAVName(fields[0], "Grantee");
        try {
            writeAttr(fname, "Grantee", val, attrType, "");
        }
        catch(err) {
            handleScriptError("ParseSantaBarbaraIndexData_8", "Error!", err, "Index Data", fields);
        }
    }

    function makeEAVS_INDEXITEM(fields) {
        var attrType = fields[1];
        var val = fields[3].trim();
        var fname = getEAVName(fields[0], "IndexItem");
        try {
            writeAttr(fname, "IndexItem", val, attrType, "");
        }
        catch(err) {
            handleScriptError("ParseSantaBarbaraIndexData_9", "Error!", err, "Index Data", fields);
        }
    }

    function makeEAVS_DOCTYPE(fields) {
        var val = fields[2].trim();
        var fname = getEAVName(fields[0], "DocumentType");
        try {
            writeAttr(fname, "DocumentType", val, "", "");
        }
        catch(err) {
            handleScriptError("ParseSantaBarbaraIndexData_10", "Error!", err, "Index Data", fields);
        }
    }
}
