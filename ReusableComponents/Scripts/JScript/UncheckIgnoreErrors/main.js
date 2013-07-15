//--------------------------------------------------------------------------------------------------
// Script commands specific for UncheckIgnoreErrors
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
        handleScriptError("InternalScriptError", "Unable to license SDK (version 9+)", err);
    }

    function uncheckAttributeRule(rule) {
        var modified = false;
        if (rule.IgnoreErrors) {
            rule.IgnoreErrors = false;
            modified = true;
        }
        if (rule.IgnoreModifierErrors) {
            rule.IgnoreModifierErrors = false;
            modified = true;
        }
        if (rule.IgnorePreprocessorErrors) {
            rule.IgnorePreprocessorErrors = false;
            modified = true;
        }
        return modified;
    }

    function uncheckAFI(afi) {
        var rules = afi.AttributeRules;
        var modified = false;
        for (var i=0; i < rules.Size(); i++) {
            if (uncheckAttributeRule(rules.At(i))) {
                modified = true;
            }
        }
        return modified;
    }

    var root = fso.getAbsolutePathName(args[0]);
    var rsdFiles = getFiles(root, true).filter(function(f){return f.Name.match(/\.rsd$/i)});

    var ruleset = new ActiveXObject("UCLIDAFCore.RuleSet");
    for (var i=0; i < rsdFiles.length; i++) {
        rsdfilename = rsdFiles[i];
        try {
            ruleset.LoadFrom(rsdfilename, true);
        }
        catch (err) {
            handleScriptError("InternalScriptError", "Unable to load ruleset", err, "RSD File", rsdfilename);
        }

        var modified = false;

        if (ruleset.IgnorePreprocessorErrors) {
            ruleset.IgnorePreprocessorErrors = false;
            modified = true;
        }
        if (ruleset.IgnoreOutputHandlerErrors) {
            ruleset.IgnoreOutputHandlerErrors = false;
            modified = true;
        }

        var map = ruleset.attributeNameToInfoMap;
        var keys = map.GetKeys();
        for (var j=0; j < keys.Size; j++) {
            if (uncheckAFI(map.GetValue(keys.Item(j)))) {
                modified = true;
            }
        }
        if (modified) {
            try {
                ruleset.SaveTo(rsdfilename, true);
            }
            catch (err) {
                handleScriptError("InternalScriptError", "Unable to save ruleset", err, "RSD File", rsdfilename);
            }
        }
    }
}
