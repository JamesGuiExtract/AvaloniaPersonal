//--------------------------------------------------------------------------------------------------
// Script commands specific for ExpandImports
//--------------------------------------------------------------------------------------------------
//
//--------------------------------------------------------------------------------------------------
// Script Description
//--------------------------------------------------------------------------------------------------
// Recursively expands any import statements in the file named by the first argument and outputs
// the result to the file named by the second argument.
//
// Usage:   ExpandImports inputDat outputDat
//              inputDat - The path to the regex file containing import statements
//              outputDat - The path to the regex file with all import statements expanded
//--------------------------------------------------------------------------------------------------

function main(args) {
    var inputDat = args[0];
    var outputDat = args[1];
    writeText(outputDat, expandImports(fso.getAbsolutePathName(inputDat)));

    //--------------------------------------------------------------------------------------------------
    // Get text replacement from a filename
    //--------------------------------------------------------------------------------------------------
    function getText(fname) {
        if (!getText.cache) {
            getText.cache = {};
        }
        if (!getText.cache[fname]) {
            var result = readAllText(fname).replace(/[\r\n]*$/, "");
            getText.cache[fname] = result;
            return result;
        }
        return getText.cache[fname];
    }

    //--------------------------------------------------------------------------------------------------
    // Replace import statments with the file's contents
    //--------------------------------------------------------------------------------------------------
    function expandImports(inputDat) {
        var text = getText(inputDat);
        var rootDir = fso.getParentFolderName(inputDat);
        var match;
        while (match = /^((\x20*)#import ([^\r\n]+?(?:\.(dat|txt|spm)))(?:\.etf)?)/im.exec(text)) {
            var statement = match[1];
            var indent = match[2];
            var filename = match[3];
            var fullpath = fso.BuildPath(rootDir, filename);
            var rep = expandImports(fullpath).replace(/^(?=\x20*\S)/mg, indent);
            expr = new RegExp("^"+statement.replace(/([\\.?$()[])/g, "\\$1"), "img");
            text = text.replace(expr, rep);
        }
        return text;
    }
}
