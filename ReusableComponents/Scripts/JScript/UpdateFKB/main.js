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

    var ruleset = new ActiveXObject("UCLIDAFCore.RuleSet");
    for (var i=0; i < rsdFiles.length; i++) {
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
        var orderMappingDBs = getFiles(root, true).filter(function(f){return f.Name.match(/^OrderMappingDB\.sdf$/i)});
        for (var i=0; i < orderMappingDBs.length; i++) {
            var databasePath = orderMappingDBs[i];
            var exporter = "\"" + exporterPath + "\" \"" + databasePath + "\"";
            args = "\"UPDATE [Settings] SET [Value] = '" + newFKB + "' WHERE [Name] = 'FKBVersion'\" _";

            // Run EXE and wait on return
            WSHShell.Run(exporter + " " + args, 0, true);
        }
    }
}
