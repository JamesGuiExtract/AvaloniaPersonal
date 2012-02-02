//--------------------------------------------------------------------------------------------------
// Script commands specific for ParseSanFranciscoAtPacIndexData
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
// Usage:   ParseSanFranciscoAtPacIndexData inputFile outputDir
//              inputFile - The path to the index data file
//              outputDir - The path to the EAV output dir
//
// Example inputFile: K:\Common\Engineering\Sample Files\AtPac\CA - San Francisco\Set003\IndexData\OriginalsFromCustomer\sf_extract_apn.txt
//--------------------------------------------------------------------------------------------------

function main(args) {
    var inputFile = fso.getAbsolutePathName(args[0]);
    var outputDir = fso.getAbsolutePathName(args[1]);
    var imageFiles = getFiles(fso.BuildPath(fso.getParentFolderName(outputDir), "Images"), true).filter(function(f){return f.Name.match(/\.(tiff?|pdf|\d{3})$/i)});
    var imageMap = {};
    for (var i=0; i < imageFiles.length; i++) {
        var f = imageFiles[i];
        imageMap[f.Name.slice(0,4)+f.Name.slice(5,12)] = f;
    }

    if (!fso.folderExists(outputDir)) {
        fso.createFolder(outputDir);
    }

    var csvlines = readAllText(inputFile).split(/\n/).map(function(s){return s.trim()}).filter(function(l){return l.match(/^(?:[^,]*,){6}/i)});
    for (var i=0; i < csvlines.length; i++) {
        handleDebug("CSVLine", i);
        var fields = csvlines[i].split(/\s*(?:-\d{2})?,\s*/);

        //makeEAVS_PIN(fields);
        makeEAVS_PARTIES(fields);
    }
    
    function appendText(fname, text) {
        // Open the file
        try {
            var f = fso.OpenTextFile(fname, 8, true);
        }
        catch(err) {
            handleScriptError("ParseSanFranciscoAtPacIndexData_3", "Unable to open output file!", err, "FileName", fname);
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
            handleScriptError("ParseSanFranciscoAtPacIndexData_4", "Can't figure out file name!",
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
        var fname = getEAVName(fields[0]+fields[1], "PIN");
        writeAttr(fname, "PIN", fields[4].trim(), "", "");
    }

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
        try {
            writeAttr(fname, typ, fields[5].trim(), "", "");
        }
        catch(err) {
            handleScriptError("ParseSanFranciscoAtPacIndexData_5", "Error!", err, "Index Data Line", fields);
        }
    }
}
