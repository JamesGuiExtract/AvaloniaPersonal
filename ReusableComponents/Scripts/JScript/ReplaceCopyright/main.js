//--------------------------------------------------------------------------------------------------
// Script commands specific for ReplaceCopyright
//--------------------------------------------------------------------------------------------------
//
//--------------------------------------------------------------------------------------------------
// Script Description
//--------------------------------------------------------------------------------------------------
// Changes copyright years in cpp, cs, rc and txt files
//
// Usage:   ReplaceCopyright rootdir oldyear newyear
//--------------------------------------------------------------------------------------------------

function main(args) {
    var root = fso.getAbsolutePathName(args[0]);
    var oldyear = args[1];
    var newyear = args[2];

    var files = getFiles(root, true).filter(function(f){return f.Name.match(/\.(cpp|cs|rc|txt)$/i)});

    // Pattern 1 of 2
    // Require special endings to guard against non-ES copyrights being replaced
    // E.g.:
    // Copyright 2014
    // (c) 2014
    // (c) 2002-2014 Extract Systems
    var replaceYear = new RegExp("((Copyright\\W+([c\\c2\\xa9]+\\W+)?|\\(c\\)\\W+)"+
        "(\\d{4}\\s?(to|-)\\s?)?)"+
        oldyear+"(?=(\\\\[0n])?\\W*($|\"|Extract\\s*Systems?|UCLID\\s*Software))","igm");

    // Pattern 2 of 2
    // Require "Extract Systems"
    // E.g., Copyright Extract Systems 2014
    var replaceYear2 = new RegExp("((Copyright\\W+([c\\c2\\xa9]+\\W+)?|\\(c\\)\\W+)Extract\\s*Systems?\\W+(LLC\\W+)?"+
        "(\\d{4}\\s?(to|-)\\s?)?)"+
        oldyear+"\\b");

    for (var i=0; i < files.length; i++) {
        var filename = files[i].Path;
        var text = readAllText(filename);

        // Reset regexes
        replaceYear.lastIndex = replaceYear2.lastIndex = 0;

        if (replaceYear.test(text) || replaceYear2.test(text)) {
          writeText(filename, text.replace(replaceYear, "$1"+newyear).replace(replaceYear2, "$1"+newyear));
        }
    }
}
