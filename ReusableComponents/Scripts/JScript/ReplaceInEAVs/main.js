//--------------------------------------------------------------------------------------------------
// Script commands specific for ReplaceInEAVs
//--------------------------------------------------------------------------------------------------
//
//--------------------------------------------------------------------------------------------------
// Script Description
//--------------------------------------------------------------------------------------------------
// Makes EAV replacements to duplicate replace strings behavior
//
// Usage:   ReplaceInEAVs patterns delimiter inputDir outputDir
//              patterns  - The path to the replacement pairs dat file
//              delimiter - The character that separates the pattern from the replacement
//              inputDir  - The path to the directory containing the EAV files
//              outputDir - The path to the directory for writing the EAVs after the replacements
//                          have been made.
//--------------------------------------------------------------------------------------------------

function main(args) {
    var patterns = args[0];
    var delimiter = args[1];
    var inputDir = args[2];
    var outputDir = args[3];

    var patterns = readAllText(patterns).split(/[\r\n]+/).
      filter(function(l){return !l.match(/^\s*\/\//)}).
      map(function(l){   var pieces = l.split(delimiter)
                         if (pieces.length != 2) {
                             throw new Error();
                         }
                         return pieces;
                      });


    if (!fso.folderExists(outputDir)) {
        fso.createFolder(outputDir);
    }

    var eavs = fso.GetFolder(inputDir).Files;
    var files = new Enumerator(eavs);
    for (; !files.atEnd(); files.moveNext()) {
        var f = files.item();
        var text = readAllText(f.Path);

        var out = [];
        var lines = text.split(/[\r\n]+/).filter(function(l){return !l.match(/^\s*\/\//)});
        for (var i=0; i < lines.length; i++) {
            l = lines[i];
            if (!l.match(/^\.*\w+\|[^|]+(\|[^|]+)?$/)) {
                out.push(l);
            } else {
                var pieces = l.split('|')
                for (var j=0; j<patterns.length; j++) {
                    p = patterns[j];
                    pieces[1] = pieces[1].replace(new RegExp(p[0], "gim"), p[1]);
                }
                out.push(pieces.join('|'));
            }
        }

        writeText(fso.BuildPath(outputDir, f.Name), out.join("\n"));
    }
}
