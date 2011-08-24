using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extract.Interop;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents the settings used for the merge ID Shield data file task.
    /// </summary>
    public class VOAFileMergeTaskSettings
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileMergeTaskSettings"/>
        /// class.
        /// </summary>
        public VOAFileMergeTaskSettings()
        {
            try
            {
                DataFile1 = "<SourceDocName>.session1.voa";
                DataFile2 = "<SourceDocName>.session2.voa";
                UseMutualOverlap = true;
                OverlapThreshold = 75;
                OutputFile = "<SourceDocName>.merged.voa";
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32059");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileMergeTaskSettings"/>
        /// class.
        /// </summary>
        /// <param name="settings">The <see cref="VOAFileMergeTaskSettings"/> to copy settings from.
        /// </param>
        public VOAFileMergeTaskSettings(VOAFileMergeTaskSettings settings)
        {
            try
            {
                DataFile1 = settings.DataFile1;
                DataFile2 = settings.DataFile2;
                UseMutualOverlap = settings.UseMutualOverlap;
                OverlapThreshold = settings.OverlapThreshold;
                OutputFile = settings.OutputFile;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32135");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileMergeTaskSettings"/> class.
        /// </summary>
        /// <param name="dataFile1">The first data file to merge.</param>
        /// <param name="dataFile2">The second data file to merge.</param>
        /// <param name="useMutualOverlap"><see langword="true"/> if
        /// <see paramref="overlapThreshold"/> must be met when comparing redaction A to B as well
        /// as B to A. <see langword="false"/> if <see paramref="overlapThreshold"/> needs to be met
        /// in only one of the two comparisons (such as a small redaction contained in a larger one.
        /// </param>
        /// <param name="overlapThreshold">The percentage of mutual overlap required to consider two
        /// redactions as equivalent.</param>
        /// <param name="outputFile">The filename where merged output should be written.</param>
        public VOAFileMergeTaskSettings(string dataFile1, string dataFile2, bool useMutualOverlap,
            int overlapThreshold, string outputFile)
        {
            try 
	        {	        
                DataFile1 = dataFile1;
                DataFile2 = dataFile2;
                UseMutualOverlap = useMutualOverlap;
                OverlapThreshold = overlapThreshold;
                OutputFile = outputFile;
	        }
	        catch (Exception ex)
	        {
		        throw ex.AsExtract("ELI32060");
	        };
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The first data file to merge.
        /// </summary>
        public string DataFile1
        {
            get;
            protected set;
        }

        /// <summary>
        /// The second data file to merge.
        /// </summary>
        public string DataFile2
        {
            get;
            protected set;
        }

        /// <summary>
        /// The percentage of mutual overlap required to consider two redactions as equivalent.
        /// </summary>
        public int OverlapThreshold
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets whether <see cref="OverlapThreshold"/> must be met when comparing 
        /// redaction A to B as well as B to A. 
        /// </summary>
        /// <value>
        /// <see langword="true"/> if redactions must mutually overlap to be merged;
        /// <see langword="false"/> if <see cref="OverlapThreshold"/> needs to be met in only one
        /// of the two comparisons. (such as a small redaction contained in a larger one)
        /// </value>
        public bool UseMutualOverlap
        {
            get;
            protected set;
        }

        /// <summary>
        /// The filename where merged output should be written.
        /// </summary>
        public string OutputFile
        {
            get;
            protected set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a <see cref="VOAFileMergeTaskSettings"/> from the specified 
        /// <see cref="IStreamReader"/>.
        /// </summary>
        /// <param name="reader">The reader from which to create the 
        /// <see cref="VOAFileMergeTaskSettings"/>.</param>
        /// <returns>A <see cref="VOAFileMergeTaskSettings"/> created from the
        /// specified <see cref="IStreamReader"/>.</returns>
        internal static VOAFileMergeTaskSettings ReadFrom(IStreamReader reader)
        {
            try
            {
                ExtractException.Assert("ELI32061", "Unable to load newer " +
                    VOAFileMergeTask._COMPONENT_DESCRIPTION + ".",
                    reader.Version <= VOAFileMergeTask._CURRENT_VERSION);

                VOAFileMergeTaskSettings settings =
                    new VOAFileMergeTaskSettings();

                settings.DataFile1 = reader.ReadString();
                settings.DataFile2 = reader.ReadString();
                settings.OverlapThreshold = reader.ReadInt32();
                settings.OutputFile = reader.ReadString();

                if (reader.Version >= 2)
                {
                    settings.UseMutualOverlap = reader.ReadBoolean();
                }

                return settings;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI32062",
                    "Unable to read replace indexed text settings.", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="VOAFileMergeTaskSettings"/> to the specified
        /// <see cref="IStreamWriter"/>.
        /// </summary>
        /// <param name="writer">The writer into which the 
        /// <see cref="VOAFileMergeTaskSettings"/> will be written.</param>
        internal virtual void WriteTo(IStreamWriter writer)
        {
            try
            {
                writer.Write(DataFile1);
                writer.Write(DataFile2);
                writer.Write(OverlapThreshold);
                writer.Write(OutputFile);
                writer.Write(UseMutualOverlap);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI32063",
                    "Unable to write create redacted text settings.", ex);
            }
        }

        #endregion Methods
    }
}
