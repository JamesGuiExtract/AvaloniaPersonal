//--------------------------------------------------------------------------------------------------
// Script commands specific for ParseAmadorAtPacIndexData
//--------------------------------------------------------------------------------------------------
//
//--------------------------------------------------------------------------------------------------
// Script Description
//--------------------------------------------------------------------------------------------------
// Creates EAVs out of the index data in the file named by the first argument and outputs
// the results to files below the directory named by the second argument.
//
// Also copies the images to subfolders by document type (assumes images are in Images dir parallel
// to output dir)
//
// Usage:   ParseAmadorAtPacIndexData inputFile outputDir
//              inputFile - The path to the index data file
//              outputDir - The path to the EAV output dir
//
// Example inputFile: K:\Common\Engineering\Sample Files\AtPac\CA - Amador\Set005\Index_Data\extract2.txt
//--------------------------------------------------------------------------------------------------

function main(args) {
    var inputFile = fso.getAbsolutePathName(args[0]);
    var outputDir = fso.getAbsolutePathName(args[1]);
    var imageFiles = fso.GetFolder(fso.BuildPath(fso.getParentFolderName(outputDir), "Images")).Files;
    var files = new Enumerator(imageFiles);
    var imageMap = {};
    for (; !files.atEnd(); files.moveNext()) {
        var f = files.item();
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
        var fields = csvlines[i].split(/\s*(?:-\d{2})?\|\s*/);

        // Update map of images to copy
        setCopyImage(fields);

        makeEAVS_PARTIES(fields);
    }
    
    copyImages(imagesToCopy);

    // Update map of images to destinations based on doc-type(s)
    function setCopyImage(fields) {
        var doctype = fields[3].replace(/[^\w\s]/g, "_").replace(/_+$/,"");
        var iname = getImageName(fields[0]+fields[1]);
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
                handleScriptError("ParseAmadorAtPacIndexData_1", "Unable to create folder!", err, "FolderName", newdir);
            }

            try {
                fso.CopyFile(iname, newdir+"\\");
            }
            catch(err) {
                handleScriptError("ParseAmadorAtPacIndexData_2", "Unable to copy file!", err, "FileName", iname, "Destination", newdir);
            }
        }
    }

    function appendText(fname, text) {
        // Open the file
        try {
            var f = fso.OpenTextFile(fname, 8, true);
        }
        catch(err) {
            handleScriptError("ParseAmadorAtPacIndexData_3", "Unable to open output file!", err, "FileName", fname);
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
            handleScriptError("ParseAmadorAtPacIndexData_4", "Can't figure out file name!",
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
    function makeEAVS_PARTIES(fields) {
        var typ = "";
        switch(fields[4]) {
            case "R": typ = "Grantor";
            break;
            case "E": typ = "Grantee";
            break;
            default: typ = "Unknown";
        }
        var fname = getEAVName(fields[0]+fields[1], typ);
        writeAttr(fname, typ, fields[5].trim(), "", "");
    }
}
