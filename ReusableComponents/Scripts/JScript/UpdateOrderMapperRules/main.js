//--------------------------------------------------------------------------------------------------
// Script commands specific for UpdateOrderMapperRules
//--------------------------------------------------------------------------------------------------
//
//--------------------------------------------------------------------------------------------------
// Script Description
//--------------------------------------------------------------------------------------------------
// Generates pattern files based on a database
//
// Usage:   UpdateOrderMapperRules databasePath outputDir
//              databasePath  - The path to the OrderMappingDB file
//              outputDir - The path to the root directory for writing the pattern files
//--------------------------------------------------------------------------------------------------

function main(args) {
    var databasePath = fso.getabsolutepathname(args[0]);
    var outputDir = fso.getabsolutepathname(args[1]);

    if (!fso.folderExists(outputDir)) {
        fso.createFolder(outputDir);
    }

    var WSHShell = new ActiveXObject("WScript.Shell");

    if (fso.fileExists("C:\\Engineering\\Binaries\\Release\\SQLCompactExporter.exe")) {
        var exporter = "\"C:\\Engineering\\Binaries\\Release\\SQLCompactExporter.exe\" \"" + databasePath + "\"";
        var importer = "\"C:\\Engineering\\Binaries\\Release\\SQLCompactImporter.exe\" \"" + databasePath + "\"";
    } else if (WSHShell.ExpandEnvironmentStrings("%ProgramFiles(x86)%") == "%ProgramFiles(x86)%") {
        var exporter = "\"C:\\Program Files\\Extract Systems\\CommonComponents\\SQLCompactExporter.exe\" \"" + databasePath + "\"";
        var importer = "\"C:\\Program Files\\Extract Systems\\CommonComponents\\SQLCompactImporter.exe\" \"" + databasePath + "\"";
    } else {
        var exporter = "\"C:\\Program Files (x86)\\Extract Systems\\CommonComponents\\SQLCompactExporter.exe\" \"" + databasePath + "\"";
        var importer = "\"C:\\Program Files (x86)\\Extract Systems\\CommonComponents\\SQLCompactImporter.exe\" \"" + databasePath + "\"";
    }

    var commonWordPattern = /\b(URINE|TOTAL|UR|ABSOLUTE|BLOOD|ABS|COUNT|RATIO|RANDOM|AUTO|UA|SERUM|EST|VITAMIN|CELL|CELLS|LEVEL|PERCENT|ESTIMATED|DNA|QUANT|QN|DIRECT|LVL|AUTO|CT|PLASMA|FASTING|CALCULATED|ABS|AUTOMATED|CONC|SURFACE|AVERAGE|TOTAL|CALC|BLD)\b/ig;

    // Add missing rows to LabOrderTest
    var orphanFile = fso.BuildPath(outputDir, "OrphanTests.txt");
    var args = "\"SELECT '_', TestCode FROM LabTest WHERE TestCode NOT IN (SELECT TestCode FROM LABORDERTEST)\" \"" + orphanFile + "\"";
    WSHShell.Run(exporter + " " + args, 0, true);

    if (fso.fileExists(orphanFile)) {
        WSHShell.Run(importer + " LabOrderTest \"" + orphanFile + "\"", 0, true);
        fso.DeleteFile(orphanFile);
    }

    // Add official names as AKAs
    var missingAKAFile = fso.BuildPath(outputDir, "MissingAKAs.txt");
    args = "\"SELECT OfficialName, TestCode FROM LabTest WHERE TestCode NOT IN (SELECT TestCode FROM AlternateTestName WHERE Name = OfficialName)\" \"" +
        missingAKAFile + "\"";
    WSHShell.Run(exporter + " " + args, 0, true);

    if (fso.fileExists(missingAKAFile)) {
        WSHShell.Run(importer + " AlternateTestName \"" + missingAKAFile + "\"", 0, true);
        fso.DeleteFile(missingAKAFile);
    }

    // Set sample type if null
    args = "\"UPDATE LabTest SET SampleType = 'Blood' WHERE SampleType IS NULL\" _";
    WSHShell.Run(exporter + " " + args, 0, true);

    function substituteProblemChars(c) {
        if (c == '%') {
            return "(%|[569][il:,.-]?[569]?)";
        } else {
            return c.replace(/[0O]/ig, "[0O]").replace(/m/ig, "(rn|m)").replace(/[il!]/ig, "[il!]").replace(/(#)/g, "\\$1");
        }
    }

    // Make testname_ars files
    function getLoosenedName(name, allowNewlines) {
        var normalizedName = name.replace(commonWordPattern, "").replace(/[^0-9a-z#%]+/ig, "");

        if (name.replace(/[^0-9a-z#%]+/ig, "").length < 3) {
            return name.replace(/([|\\.^$(){}\[\]?*+#])/g, "\\$1").replace(/\x20/g, "\\x20").replace(/\t/g, "\\t").replace(/\r/g, "\\r").replace(/\n/g, "\\n");
        }

        if (allowNewlines && normalizedName.length > 3) {
            if (normalizedName.length > 6) {
                var errorAllowance = "(?'_e'){2}";
            } else {
                var errorAllowance = "(?'_e')";
            }
            var firstWord = true;
            return "(?#"+name.replace(/[()]/g,"_")+")"
              + errorAllowance
              + name
              .split(/[^0-9a-z#%]+/ig)
              .map(function(w) {
                  var index = 0;
                  return w.split("")
                  .map(function(c) {
                      c = substituteProblemChars(c);
                      // Don't actually allow newlines if really short (just allow errors)
                      if (index == 0 && name.length > 6) {
                          var result = "((([^\\xAB\\xBB\\s](?>(?'-_m')|(?'-_e'))|(\\x20(?!\\x20)|[_\\W-[\\s\\xAB\\xBB]])){0,4}?"
                                     + "|(\\r\\n){2}|[_\\W-[\\s\\xAB\\xBB]]?\\r\\n[_\\W-[\\s\\xAB\\xBB]]?";
                          // Allow name to have values, etc in between parts if the name is long enough
                          if (!firstWord && name.length > 12) {
                              result += "|\\x20{4}[^\\r\\n]+[\\r\\n]+";
                          }
                          result += (")" + c + "(?(_m)(?'-_m'))|(?'-_e')(?'_m'))");
                      } else {
                          var result = "(([^\\xAB\\xBB\\s](?>(?'-_m')|(?'-_e'))|(\\x20(?!\\x20)|[_\\W-[\\x20\\r\\n\\t\\xAB\\xBB]])){0,4}?"
                                       + c + "(?(_m)(?'-_m'))|(?'-_e')(?'_m'))";
                      }
                      index += 1;
                      firstWord = false;
                      return result;
                  })
                  .join("");
              }).join("")
              + "(\\x20(?!\\x20)|[_\\W-[\\x20\\r\\n\\t\\xAB\\xBB]]|[^\\xAB\\xBB\\s](?'-_e')){0,4}?(?<=\\S)";
        } else if (allowNewlines) {
            return "(?#"+name.replace(/[()]/g,"_")+")"
              + "[_\\W-[\\x20\\r\\n\\t\\xAB\\xBB]]{0,4}"
              + name
              .split(/[^0-9a-z#%]+/ig)
              .map(function(w) {
                  return w.split("")
                  .map(function(c) {
                      return substituteProblemChars(c);
                  })
                  .join("(?(_m)(?!)|(\\x20(?!\\x20)|[_\\W-[\\x20\\r\\n\\t\\xAB\\xBB]]){0,4})");
              }).join("(?(_m)(?!)|(\\x20(?!\\x20)|[_\\W-[\\x20\\t\\xAB\\xBB]]){0,4})")
              + "([^\\xAB\\xBB\\s](?'-_e')){0,2}?(\\x20(?!\\x20)|[_\\W-[\\x20\\r\\n\\t\\xAB\\xBB]]){0,4}?";
        } else {
            return "(?#"+name.replace(/[()]/g,"_")+")(?>"
              + "[_\\W-[\\x20\\r\\n\\t\\xAB\\xBB]]{0,4}"
              + name.replace(/[^0-9a-z#%]+/ig, "")
              .split("")
              .map(function(c) {
                  return substituteProblemChars(c);
              })
              .join("(\\x20(?!\\x20)|[_\\W-[\\x20\\r\\n\\t\\xAB\\xBB]]){0,4}")
              + ")(\\x20(?!\\x20)|[_\\W-[\\x20\\r\\n\\t\\xAB\\xBB]]){0,4}?";
        }
    }

    var tempFile = fso.BuildPath(outputDir, "___updateOMTempFile___.txt");
    args = "\"SELECT UPPER(Name) FROM AlternateTestName\" \"" + tempFile + "\"";
    WSHShell.Run(exporter + " " + args, 0, true);

    var m = {};
    var akas = [];
    var akasWithNewLines = [];
    readAllText(tempFile).split(/[\r\n]+/).
        map(function(l) {
            var needAKA2 = l.replace(commonWordPattern, "").replace(/[^0-9a-z#%]+/ig, "").length > 3;
            var aka = getLoosenedName(l, false);
            if (needAKA2) {
                var aka2 = getLoosenedName(l, true);
            }
            if (!m[aka]) {
                m[aka] = true;
                akas.push(aka);
                if (needAKA2) {
                    akasWithNewLines.push(aka2);
                }
            }
        });

    // Delete temp file
    if (fso.fileExists(tempFile)) {
        fso.DeleteFile(tempFile);
    }

    var filePrefix = "(?inxs)\r\n(?=\\S)(?>^|(?<=[\\x20]{2}|^([-=>,.]{0,4}|.)?[-=>,.]\\x20([*]+\\x20)?|^([*]+|\\d{4})\\x20))(?'p'([^>\\s\\xAB\\xBB]\\s?)??)(?'t'\r\n    ";
    var rowDelimeter = "\r\n  | ";
    var fileSuffix = "\r\n)\r\n(?=([\\d'](?#=superscript))?(-EXT\\b|!)?(([-/,.:]|\\s?\\x28[^\\x0D\\x0A\\x29]+\\x29:?|[*]+)?(\\s?$|\\x20)|\\d(\\.\\d)?\\s))";

    writeText(fso.BuildPath(fso.GetParentFolderName(outputDir), "testnames_ars.dat"), filePrefix + akas.sort(function(a, b) {
        return b.length - a.length;
    }).join(rowDelimeter) + fileSuffix);

    filePrefix = "(?inxs)\r\n^(?=[^\\s\\xAB\\xBB])(?'t'\r\n    ";
    var fileSuffix = "\r\n)\r\n(?=([\\d'](?#=superscript))?(-EXT\\b|!)?(([-/,.:]|\\s?\\x28[^\\x0D\\x0A\\x29]+\\x29:?|[*]+)?(\\s?$|\\x20{2})|\\d(\\.\\d)?\\x20{2}))";
    writeText(fso.BuildPath(fso.GetParentFolderName(outputDir), "testnames_ars2.dat"), filePrefix + akasWithNewLines.sort(function(a, b) {
        return b.length - a.length;
    }).join(rowDelimeter) + fileSuffix);



    // Write out dat files for order mapper RSD

    // Official names

    // Blood
    function getLoosenedOfficialName(name) {
        return name.replace(/[^0-9a-z#%]+/ig, "[^0-9A-Z`]*").replace(/(#)/g, "\\$1");
    }

    args = "\"SELECT TestCode, OfficialName FROM LabTest WHERE NOT SampleType = 'Urine' ORDER BY TestCode ASC\" \"" + tempFile + "\"";
    WSHShell.Run(exporter + " " + args, 0, true);
    var officialNames = [];
    readAllText(tempFile).split(/[\r\n]+/).
        map(function(l) {
            var pieces = l.split("\t");
            var testcode = pieces[0];
            var name = getLoosenedOfficialName(pieces[1]);
            officialNames.push("(?(a"+testcode+")(?!)|(?>(?'a"+testcode+"'"+name+"`[^|]*))) //" + pieces[0]);
        });

    // Delete temp file
    if (fso.fileExists(tempFile)) {
        fso.DeleteFile(tempFile);
    }

    var filePrefix = "  ";
    var rowDelimeter = "\r\n| ";
    var fileSuffix = "";

    writeText(fso.BuildPath(outputDir, "Blood\\OfficialNames.dat"), filePrefix + officialNames.join(rowDelimeter) + fileSuffix);

    // Urine
    args = "\"SELECT TestCode, OfficialName FROM LabTest WHERE SampleType = 'Urine' ORDER BY TestCode ASC\" \"" + tempFile + "\"";
    WSHShell.Run(exporter + " " + args, 0, true);
    var officialNames = [];
    readAllText(tempFile).split(/[\r\n]+/).
        map(function(l) {
            var pieces = l.split("\t");
            var testcode = pieces[0];
            var name = getLoosenedOfficialName(pieces[1]);
            officialNames.push("(?(a"+testcode+")(?!)|(?>(?'a"+testcode+"'"+name+"`[^|]*)))");
        });

    // Delete temp file
    if (fso.fileExists(tempFile)) {
        fso.DeleteFile(tempFile);
    }

    var filePrefix = "  ";
    var rowDelimeter = "\r\n| ";
    var fileSuffix = "";

    writeText(fso.BuildPath(outputDir, "Urine\\OfficialNames.dat"), filePrefix + officialNames.join(rowDelimeter) + fileSuffix);


    // Determinate/Indeterminate

    function getLoosenedAKA(name, allowErrors) {
        var normalizedName = name.replace(commonWordPattern, "").replace(/[^0-9a-z#%]+/ig, "");
        if (allowErrors && normalizedName.length > 3) {
            if (normalizedName.length > 6) {
                var errorAllowance = "(?'_e'){2}";
                var cleanupErrorAllowance = "((?(_e)(?'-_e'))(?(_m)(?'-_m'))){2}";
            } else {
                var errorAllowance = "(?'_e')";
                var cleanupErrorAllowance = "(?(_e)(?'-_e'))(?(_m)(?'-_m'))";
            }
            var index = 0;
            var chars = name.replace(/[^0-9a-z#%]+/ig, "").split("");
            return errorAllowance
              + "[^0-9A-Z#%`]{0,4}?"
              + chars.map(function(c) {
                  c = substituteProblemChars(c);
                  var ret = "(([^#%`](?>(?'-_m')|(?'-_e'))|[^0-9a-z#%`]){0,4}?"+c;
                  if (index < chars.length - 1) {
                    index += 1;
                    return ret + "(?(_m)(?'-_m'))|(?'-_e')(?'_m'))";
                  } else {
                    return ret + "(?(_m)(?'-_m'))|(?'-_e')(?'_m')([^0-9a-z#%`]{0,4}?[^#%`](?>(?'-_m')|(?'-_e')))?)";
                  }
              })
              .join("")
              + "([^#%`](?>(?'-_m')|(?'-_e'))){0,2}"
              + cleanupErrorAllowance
              + "[^0-9A-Z#%`]{0,4}";
        } else {
            return "(?>[^0-9A-Z#%`]{0,4}?"
              + name.replace(/[^0-9a-z#%]+/ig, "")
              .split("")
              .map(function(c) {
                  c = substituteProblemChars(c);
                  return c;
              })
              .join("[_\\W]{0,4}")
              + ")[^0-9A-Z#%`]{0,4}";
        }
    }

    // Make Blood map
    var bloodAKAs = {};

    args = "\"SELECT a.TestCode, UPPER(a.Name), COALESCE(c.frequency,'0') AS Freq FROM AlternateTestName a JOIN LabTest l on a.TestCode = l.TestCode " +
        "LEFT OUTER JOIN AKAFrequency c on a.TestCode = c.TestCode AND a.Name = c.AKA WHERE NOT SampleType = 'Urine'\" \"" + tempFile + "\"";
    WSHShell.Run(exporter + " " + args, 0, true);
    readAllText(tempFile).split(/[\r\n]+/).
        map(function(l) {
            var pieces = l.split("\t");
            var testcode = pieces[0];
            var name = getLoosenedAKA(pieces[1], false);
            var freq = parseInt(pieces[2], 10);
            if (bloodAKAs[name] === undefined) {
                var m = {};
                m[testcode] = [pieces[1], freq];
                bloodAKAs[name] = m;
            } else if (bloodAKAs[name][testcode] === undefined) {
                bloodAKAs[name][testcode] = [pieces[1], freq];
            }
        });

    // Delete temp file
    if (fso.fileExists(tempFile)) {
        fso.DeleteFile(tempFile);
    }

    // Make Urine map
    var urineAKAs = {};

    args = "\"SELECT a.TestCode, UPPER(a.Name), COALESCE(c.frequency,'0') AS Freq FROM AlternateTestName a JOIN LabTest l on a.TestCode = l.TestCode " +
        "LEFT OUTER JOIN AKAFrequency c on a.TestCode = c.TestCode AND a.Name = c.AKA WHERE SampleType = 'Urine'\" \"" + tempFile + "\"";
    WSHShell.Run(exporter + " " + args, 0, true);
    readAllText(tempFile).split(/[\r\n]+/).
        map(function(l) {
            var pieces = l.split("\t");
            var testcode = pieces[0];
            var name = getLoosenedAKA(pieces[1], false);
            var freq = parseInt(pieces[2], 10);
            if (urineAKAs[name] === undefined) {
                var m = {};
                m[testcode] = [pieces[1], freq];
                urineAKAs[name] = m;
            } else if (urineAKAs[name][testcode] === undefined) {
                urineAKAs[name][testcode] = [pieces[1], freq];
            }
        });

    // Delete temp file
    if (fso.fileExists(tempFile)) {
        fso.DeleteFile(tempFile);
    }

    // Separate into determinate and indeterminate lists

    // Blood
    var bloodD = [];
    var bloodI = [];
    var bloodF = [];
    for (var aka in bloodAKAs) {
        var codes = bloodAKAs[aka];
        if (Object.size(codes) > 1 || urineAKAs[aka] !== undefined) {
            for (var testcode in codes) {
                var freq = codes[testcode][1];
                bloodI.push([testcode, aka, freq]);
                if (codes[testcode][0].replace(/\W+/g,"").length > 3) {
                    var name = getLoosenedAKA(codes[testcode][0], true);
                    bloodF.push([testcode, name, freq]);
                }
            }
        } else {
            for (var testcode in codes) {
                var freq = codes[testcode][1];
                bloodD.push([testcode, aka, freq]);
                if (codes[testcode][0].replace(/\W+/g,"").length > 3) {
                    var name = getLoosenedAKA(codes[testcode][0], true);
                    bloodF.push([testcode, name, freq]);
                }
            }
        }
    }

    // Urine
    var urineD = [];
    var urineI = [];
    var urineF = [];
    for (var aka in urineAKAs) {
        var codes = urineAKAs[aka];
        if (Object.size(codes) > 1 || bloodAKAs[aka] !== undefined) {
            for (var testcode in codes) {
                var freq = codes[testcode][1];
                urineI.push([testcode, aka, freq]);
                if (codes[testcode][0].replace(/\W+/g,"").length > 3) {
                    var name = getLoosenedAKA(codes[testcode][0], true);
                    urineF.push([testcode, name, freq]);
                }
            }
        } else {
            for (var testcode in codes) {
                var freq = codes[testcode][1];
                urineD.push([testcode, aka, freq]);
                if (codes[testcode][0].replace(/\W+/g,"").length > 3) {
                    var name = getLoosenedAKA(codes[testcode][0], true);
                    urineF.push([testcode, name, freq]);
                }
            }
        }
    }

    // Output dat files

    var filePrefix = "  ";
    var rowDelimeter = "\r\n| ";
    var fileSuffix = "";

    // Blood
    // Determinate
    bloodD = bloodD.sort(function(a, b) {
        return b[2] - a[2];
    }).map(function(r) {
        var testcode = r[0];
        var name = r[1];
        return "(?(a"+testcode+")(?!)|(?>(?'a"+testcode+"'"+name+"`[^|]*)))";
    });

    writeText(fso.BuildPath(outputDir, "Blood\\Determinate.dat"), filePrefix + bloodD.join(rowDelimeter) + fileSuffix);

    // Indeterminate
    bloodPlus = [];
    bloodI = bloodI.sort(function(a, b) {
        if (a[2] == b[2]) {
            return a[0].localeCompare(b[0]);
        } else {
            return b[2] - a[2];
        }
    }).map(function(r) {
        var testcode = r[0];
        var name = r[1];

        var featuresFile = fso.BuildPath(outputDir, "Features\\"+testcode+".dat");
        if (!fso.FileExists(featuresFile)) {
            writeText(featuresFile, "(?!)");
        }
        bloodPlus.push("(?(a"+testcode+")(?!)|(?>(?'a"+testcode+"'"+name+
            "\r\n     (?=\r\n       #import ..\\Features\\"+testcode+".dat.etf\r\n     )`[^|]*\r\n  )))");
        return "(?(a"+testcode+")(?!)|(?>(?'a"+testcode+"'"+name+"`[^|]*)))";
    });

    writeText(fso.BuildPath(outputDir, "Blood\\Indeterminate.dat"), filePrefix + bloodI.join(rowDelimeter) + fileSuffix);
    writeText(fso.BuildPath(outputDir, "Blood\\MatchTestNamesPlus.dat"), filePrefix + bloodPlus.join(rowDelimeter) + fileSuffix);

    // Fuzzy
    bloodF = bloodF.sort(function(a, b) {
        if (a[2] == b[2]) {
            return a[0].localeCompare(b[0]);
        } else {
            return b[2] - a[2];
        }
    }).map(function(r) {
        var testcode = r[0];
        var name = r[1];
        return "(?(a"+testcode+")(?!)|(?>(?'a"+testcode+"')(?'a"+testcode+"_Fuzzy'"+name+"`[^|]*)))";
    });

    writeText(fso.BuildPath(outputDir, "Blood\\Fuzzy.dat"), filePrefix + bloodF.join(rowDelimeter) + fileSuffix);


    // Urine
    // Determinate
    urineD = urineD.sort(function(a, b) {
        return b[2] - a[2];
    }).map(function(r) {
        var testcode = r[0];
        var name = r[1];
        return "(?(a"+testcode+")(?!)|(?>(?'a"+testcode+"'"+name+"`[^|]*)))";
    });

    writeText(fso.BuildPath(outputDir, "Urine\\Determinate.dat"), filePrefix + urineD.join(rowDelimeter) + fileSuffix);

    // Indeterminate
    urinePlus = [];
    urineI = urineI.sort(function(a, b) {
        if (a[2] == b[2]) {
            return a[0].localeCompare(b[0]);
        } else {
            return b[2] - a[2];
        }
    }).map(function(r) {
        var testcode = r[0];
        var name = r[1];

        var featuresFile = fso.BuildPath(outputDir, "Features\\"+testcode+".dat");
        if (!fso.FileExists(featuresFile)) {
            writeText(featuresFile, "(?!)");
        }
        urinePlus.push("(?(a"+testcode+")(?!)|(?>(?'a"+testcode+"'"+name+
            "\r\n     (?=\r\n       #import ..\\Features\\"+testcode+".dat.etf\r\n     )`[^|]*\r\n  )))");
        return "(?(a"+testcode+")(?!)|(?>(?'a"+testcode+"'"+name+"`[^|]*)))";
    });

    writeText(fso.BuildPath(outputDir, "Urine\\Indeterminate.dat"), filePrefix + urineI.join(rowDelimeter) + fileSuffix);
    writeText(fso.BuildPath(outputDir, "Urine\\MatchTestNamesPlus.dat"), filePrefix + urinePlus.join(rowDelimeter) + fileSuffix);

    // Fuzzy
    urineF = urineF.sort(function(a, b) {
        if (a[2] == b[2]) {
            return a[0].localeCompare(b[0]);
        } else {
            return b[2] - a[2];
        }
    }).map(function(r) {
        var testcode = r[0];
        var name = r[1];
        return "(?(a"+testcode+")(?!)|(?>(?'a"+testcode+"')(?'a"+testcode+"_Fuzzy'"+name+"`[^|]*)))";
    });

    writeText(fso.BuildPath(outputDir, "Urine\\Fuzzy.dat"), filePrefix + urineF.join(rowDelimeter) + fileSuffix);

    var translateFile = fso.BuildPath(outputDir, "translate.dat");
    args = " \"SELECT ('a'+a.TestCode), a.OfficialName FROM LabTest a UNION "
            + "SELECT ('a'+b.TestCode+'_Fuzzy'), (b.OfficialName+'_Fuzzy') FROM LabTest b\" \""+ translateFile + "\" /cd |";
    WSHShell.Run(exporter + args, 0, true);

    var largeValuesFile = fso.BuildPath(fso.GetParentFolderName(outputDir), "largeValueTests.dat");
    args = " \"SELECT a.Name FROM AlternateTestName a JOIN LabTest l on a.TestCode = l.TestCode " +
        "WHERE OrderOfMagnitude >= 4\" \"" + largeValuesFile + "\" /fp \"\\b( \" /fs \"\\r\\n)\\b\" /rd \"\\r\\n  | \" /esc";
    WSHShell.Run(exporter + args, 0, true);
}
