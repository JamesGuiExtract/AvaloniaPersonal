using System;
using EnvDTE;
using EnvDTE80;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Extract.SourceControl;
using System.Globalization;

namespace LICode
{
    /// <summary>
    /// The type of the location identifier.
    /// </summary>
    public enum LIType
    {
        /// <summary>
        /// Exception location identifier
        /// </summary>
        Exception = 0,

        /// <summary>
        /// Method location identifier
        /// </summary>
        Method = 1
    }

    // Handles the retrieval and modification of the Location Identifier dat files
    class LICodeHandler : IDisposable
    {
        // Location identifier number format string (five digits)
        const string LI_NUM_FORMAT = "D5";

        // Location identifier dat file data type
        class LIDatFileData
        {
            // Source safe path to the dat file (eg. $\Engineering\file.ext)
            public string vssPath;

            // Source safe item corresponding to dat file
            public ISourceControlItem vssItem;

            // String builder containing lines to append to dat file for new LI codes
            public StringBuilder builder;

            // The prefix of the location identifier code (Exception or Method)
            public string LIPrefix;

            // The location identifier number (eg. 12345 in ELI12345)
            public int LINum = -1;

            // Source safe path and location identifier prefix must be specified at instantiation
            public LIDatFileData(string newVssPath, string newLIPrefix)
            {
                vssPath = newVssPath;
                LIPrefix = newLIPrefix;
            }
        }

        // Visual SourceSafe database
        ISourceControl vssDB;

        // Array of location identifier dat file data objects
        LIDatFileData[] LIDatFiles = { new LIDatFileData("$/Engineering/ProductDevelopment/Common/UCLIDExceptionLocationIdentifiers.dat", "ELI"), new LIDatFileData("$/Engineering/ProductDevelopment/Common/ExtractMethodLocationIdentifiers.dat", "MLI") };

        public LICodeHandler() : this(null)
        {
        }

        public LICodeHandler(ISourceControl vssDatabase)
        {
            vssDB = vssDatabase;
        }

        public string NextELICode
        {
            get
            {
                // open the Visual SourceSafe database if not already opened
                if (vssDB == null)
                {
                    vssDB = SourceControlMethods.OpenSourceControlDatabase();
                }

                // retrieve the Exception dat file if it has not already been retrieved
                RetrieveLIDatFileFromVSS(LIDatFiles[(int)LIType.Exception]);

                return GetNextLICode((int)LIType.Exception);
            }
        }

        public string NextMLICode
        {
            get
            {
                // open the Visual SourceSafe database if not already opened
                if (vssDB == null)
                {
                    vssDB = SourceControlMethods.OpenSourceControlDatabase();
                }

                // retrieve the Method dat file if it has not already been retrieved
                RetrieveLIDatFileFromVSS(LIDatFiles[(int)LIType.Method]);

                return GetNextLICode((int)LIType.Method);
            }
        }

        public string ReplaceLICodesInText(string textToReplace)
        {

            // open the Visual SourceSafe database if not already opened
            if (vssDB == null)
            {
                vssDB = SourceControlMethods.OpenSourceControlDatabase();
            }

            // retrieve both dat files
            foreach (LIDatFileData datFile in LIDatFiles)
            {
                // get the vss item for the LI dat file if not already retrieved
                RetrieveLIDatFileFromVSS(datFile);
            }

            // replace the selected text with new LI codes
            Regex regex = new Regex("\"(M|E)LI\\d+\"", RegexOptions.Compiled);
            return regex.Replace(textToReplace, ReplaceLI);
        }

        public void CommitChanges()
        {
            foreach (LIDatFileData datFile in LIDatFiles)
            {
                // check if the LIBuilder exists
                if (datFile.builder != null)
                {
                    // check if any changes to the LI dat file need to be committed
                    if (datFile.builder.Length > 0)
                    {
                        // add newly added LI codes to the dat file and check it in
                        File.AppendAllText(datFile.vssItem.LocalSpec, datFile.builder.ToString());
                        datFile.vssItem.CheckIn();
                    }
                    else
                    {

                        // undo the checkout on the LI dat file
                        datFile.vssItem.UndoCheckOut();
                    }

                    // reset the values of the current LI number and the LI string builder
                    datFile.LINum = -1;
                    datFile.builder = null;
                    datFile.vssItem = null;
                }
            }
        }

        static int GetLastLICodeInDatFile(string filename)
        {
            // open the LI dat file as a comma-delimited file
            using (StreamReader reader = new StreamReader(filename))
            {
                // Iterate through each line until the end of the file
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                }

                // Get the LI number of the last entry
                int index = line.IndexOf(',');
                if (index > 2)
                {
                    string code = line.Substring(3, index - 3);
                    return int.Parse(code, CultureInfo.CurrentCulture);
                }
            }

            return -1;
        }

        string GetNextLICode(int index)
        {
            // increment the LI number
            LIDatFiles[index].LINum += 1;

            // create a new line for this LI code
            LIDatFiles[index].builder.Append(LIDatFiles[index].LIPrefix 
                + LIDatFiles[index].LINum.ToString(LI_NUM_FORMAT, CultureInfo.CurrentCulture) 
                + "," + Environment.UserName + "," + Environment.MachineName + "," 
                + System.DateTime.Now.ToString("MM/dd/yyyy,HH:mm:ss", CultureInfo.InvariantCulture) 
                + ",," + Environment.NewLine);

            // return this LI number
            return "\"" + LIDatFiles[index].LIPrefix 
                + LIDatFiles[index].LINum.ToString(LI_NUM_FORMAT, CultureInfo.CurrentCulture) + "\"";
        }

        string ReplaceLI(Match match)
        {
            // TODO: add progress status

            // get the LI dat file data associated with this match
            int i = (int)(match.Value[1] == 'E' ? 0 : 1);

            // get increment and return the LI code number
            return GetNextLICode(i);
        }

        void RetrieveLIDatFileFromVSS(LIDatFileData datFile)
        {
            if (datFile.LINum == -1)
            {
                datFile.vssItem = SourceControlMethods.CheckOutItemFromPath(vssDB, datFile.vssPath);
                datFile.builder = new StringBuilder();

                // get the last LI code from the dat file
                datFile.LINum = GetLastLICodeInDatFile(datFile.vssItem.LocalSpec);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="LICodeHandler"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="LICodeHandler"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="LICodeHandler"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (LIDatFiles != null)
                {
                    // undo the check out of the Exception dat file if it is currently held
                    foreach (LIDatFileData datFile in LIDatFiles)
                    {
                        if (datFile != null && datFile.vssItem != null && datFile.vssItem.IsCheckedOutToMe)
                        {
                            datFile.vssItem.UndoCheckOut();
                        }
                    }
                    LIDatFiles = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}
