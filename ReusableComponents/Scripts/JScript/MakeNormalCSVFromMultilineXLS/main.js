//--------------------------------------------------------------------------------------------------
// Script commands specific for CreateCSV.js
//--------------------------------------------------------------------------------------------------
//
//--------------------------------------------------------------------------------------------------
// Script Description
//--------------------------------------------------------------------------------------------------
// Creates Output.csv from a spreadsheet where some columns have only one line but other columns have
// multiple lines. Duplicates single lines so that the output has a value in each column (if possible)
// Requires Excel 2010+
//
// Script arguments:
//              spreadsheet - The name of the spreadsheet containing the data
//
//--------------------------------------------------------------------------------------------------

function main(args) {
    handleDebug("Command-Line Arguments", args.join(","));

    var spreadsheet = fso.GetAbsolutePathName(args[0]);
    handleDebug("Spreadsheet from Command-Line", spreadsheet);

    var outputDir = fso.GetParentFolderName(spreadsheet);
    handleDebug("Output Dir from Command-Line", outputDir);

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
        catch(err) { handleScriptError("CreateCSV_1", "Unable to open output file!", err, "FileName", fname); }
        // Write to the file
        f.Write(text);
        f.Close();
    }

    //--------------------------------------------------------------------------------------------------
    // Line writer
    //--------------------------------------------------------------------------------------------------
    function writeLine(fname, values) {
        var indent = ""
        if (fso.fileExists(fname)) {
            indent = "\r\n"+indent;
        }
        appendText(fname, indent+(values.join("\t")));
    }

    function getCSVName(partialName) {
        return fso.BuildPath(outputDir, partialName+".csv");;
    }

    // Create Excel Application Object
    try {
        var excel = new ActiveXObject("Excel.Application");
    }
    catch(err) {
        handleScriptError("CreateCSV_2", "Unable to create Excel Application Object!", err);
    }

    try {
      excel.DisplayAlerts = false;
      
      if (fso.fileExists(spreadsheet)) {
          try {
              var book = excel.Workbooks.Open(spreadsheet);
          }
          catch(err) {
              handleScriptError("CreateCSV_3", "Unable to open workbook!", err);
          }
      }
      else {
          try {
              throw new Error(1000000000, "Spreadsheet doesn't exist!");
          }
          catch(err) {
              handleScriptError("CreateCSV_4", "Unable to open workbook!", err);
          }
      }

      var sheet = book.Worksheets(1);
      var lastCol = sheet.UsedRange.Columns.Count;

      function getValue(columnNumber, row) {
          var range = sheet.Range(sheet.Cells(row, columnNumber),  sheet.Cells(row, columnNumber));
          return range.Item(1,1).Value;
      }

      // For each line in the spreadsheet, write:
      // One or more rows to Output.csv
      var eof = sheet.UsedRange.Rows.Count + 1;
      var outputCSV = getCSVName("Output");
      for (var curRow=1; curRow < eof; curRow++) {

          function splitValues(value) {
              if (/\n/.test(value)) {
                  return value.split('\n');
              }
              else
              {
                  return [value];
              }
          }

          var cols = [];
          var numLines = 0;
          for (var col = 1; col < lastCol+1; col++) {
              var lines = splitValues(getValue(col, curRow));
              cols[col-1] = lines;
              if (lines.length > numLines) {
                  numLines = lines.length;
              }
          }

          var lastLine = [];
          for (var line=0; line < numLines; line++) {
              var res = [];
              for (var col = 0; col < cols.length; col++) {
                  if (cols[col][line] !== undefined) {
                      lastLine[col] = cols[col][line];
                  }
                  res[col] = lastLine[col];
              }
              writeLine(outputCSV, res);
          }
      }

    }
    catch(err) {
        excel.Quit();
        handleScriptError("CreateCSV_5", "Unknown Error!", err)
        throw err;
    }
    excel.Quit();
}
