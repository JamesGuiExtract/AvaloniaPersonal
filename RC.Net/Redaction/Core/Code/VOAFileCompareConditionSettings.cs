using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extract.Interop;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents the settings used for the compare ID Shield data file condition.
    /// </summary>
    public class VOAFileCompareConditionSettings
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileCompareConditionSettings"/>
        /// class.
        /// </summary>
        public VOAFileCompareConditionSettings()
        {
            try
            {
                ConditionMetIfMatching = true;
                DataFile1 = "<SourceDocName>.session1.voa";
                DataFile2 = "<SourceDocName>.session2.voa";
                OverlapThreshold = 75;
                CreateOutput = false;
                OutputFile = "<SourceDocName>.merged.voa";
                CreateOutputOnlyOnCondition = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31766");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileCompareConditionSettings"/>
        /// class.
        /// </summary>
        /// <param name="settings">The <see cref="VOAFileCompareConditionSettings"/> to copy
        /// settings from.</param>
        public VOAFileCompareConditionSettings(VOAFileCompareConditionSettings settings)
        {
            try
            {
                ConditionMetIfMatching = settings.ConditionMetIfMatching;
                DataFile1 = settings.DataFile1;
                DataFile2 = settings.DataFile2;
                OverlapThreshold = settings.OverlapThreshold;
                CreateOutput = settings.CreateOutput;
                OutputFile = settings.OutputFile;
                CreateOutputOnlyOnCondition = settings.CreateOutputOnlyOnCondition;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32134");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileCompareConditionSettings"/> class.
        /// </summary>
        /// <param name="conditionMetIfMatching"><see langword="true"/> if the condition should
        /// return <see langword="true"/> when the <see paramref="dataFile1"/> and
        /// <see paramref="dataFile2"/> match; <see langword="false"/> if the condition should
        /// return <see langword="true"/> when they differ.</param>
        /// <param name="dataFile1">The first data file to compare.</param>
        /// <param name="dataFile2">The second data file to compare.</param>
        /// <param name="overlapThreshold">The percentage of mutual overlap required to consider two
        /// redactions as equivalent.</param>
        /// <param name="createOutput"><see langword="true"/> if a merged ID Shield data output file
        /// should be created.</param>
        /// <param name="outputFile">The filename where merged output should be written.</param>
        /// <param name="createOutputOnlyOnCondition"><see langword="true"/> to create the merged
        /// output only when the condition succeeds; <see langword="false"/> to always create the
        /// merged output.</param>
        public VOAFileCompareConditionSettings(bool conditionMetIfMatching, string dataFile1,
            string dataFile2, int overlapThreshold, bool createOutput, string outputFile,
            bool createOutputOnlyOnCondition)
        {
            try 
	        {	        
		        ConditionMetIfMatching = conditionMetIfMatching;
                DataFile1 = dataFile1;
                DataFile2 = dataFile2;
                OverlapThreshold = overlapThreshold;
                CreateOutput = createOutput;
                OutputFile = outputFile;
                CreateOutputOnlyOnCondition = createOutputOnlyOnCondition;
	        }
	        catch (Exception ex)
	        {
		        throw ex.AsExtract("ELI31767");
	        };
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets a value indicating in what circumstances the condition should be considered met.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the condition should return <see langword="true"/> when the
        /// <see paramref="dataFile1"/> and <see paramref="dataFile2"/> match;
        /// <see langword="false"/> if the condition should return <see langword="true"/> when they
        /// differ.</value>
        public bool ConditionMetIfMatching
        {
            get;
            private set;
        }

        /// <summary>
        /// The first data file to compare.
        /// </summary>
        public string DataFile1
        {
            get;
            private set;
        }

        /// <summary>
        /// The second data file to compare.
        /// </summary>
        public string DataFile2
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the percentage of mutual overlap required to consider two redactions as equivalent.
        /// </summary>
        public int OverlapThreshold
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether to generate merged output.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if a merged ID Shield data output file
        /// should be created; <see langword="false"/> otherwise.
        /// </value>
        public bool CreateOutput
        {
            get;
            private set;
        }

        /// <summary>
        /// The filename where merged output should be written.
        /// </summary>
        public string OutputFile
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the output should only be generated when the condition
        /// is met.
        /// </summary>
        /// <value><see langword="true"/> to create the merged output only when the condition
        /// succeeds; <see langword="false"/> to always create the merged output.
        /// </value>
        public bool CreateOutputOnlyOnCondition
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a <see cref="VOAFileCompareConditionSettings"/> from the specified 
        /// <see cref="IStreamReader"/>.
        /// </summary>
        /// <param name="reader">The reader from which to create the 
        /// <see cref="VOAFileCompareConditionSettings"/>.</param>
        /// <returns>A <see cref="VOAFileCompareConditionSettings"/> created from the
        /// specified <see cref="IStreamReader"/>.</returns>
        public static VOAFileCompareConditionSettings ReadFrom(IStreamReader reader)
        {
            try
            {
                ExtractException.Assert("ELI31733", "Unable to load newer " +
                    VOAFileCompareCondition._COMPONENT_DESCRIPTION + ".",
                    reader.Version <= VOAFileCompareCondition._CURRENT_VERSION);

                VOAFileCompareConditionSettings settings =
                    new VOAFileCompareConditionSettings();

                settings.ConditionMetIfMatching = reader.ReadBoolean();
                settings.DataFile1 = reader.ReadString();
                settings.DataFile2 = reader.ReadString();
                settings.OverlapThreshold = reader.ReadInt32();
                settings.CreateOutput = reader.ReadBoolean();
                settings.OutputFile = reader.ReadString();
                settings.CreateOutputOnlyOnCondition = reader.ReadBoolean();

                return settings;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI31734",
                    "Unable to read replace indexed text settings.", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="VOAFileCompareConditionSettings"/> to the specified
        /// <see cref="IStreamWriter"/>.
        /// </summary>
        /// <param name="writer">The writer into which the 
        /// <see cref="VOAFileCompareConditionSettings"/> will be written.</param>
        public void WriteTo(IStreamWriter writer)
        {
            try
            {
                writer.Write(ConditionMetIfMatching);
                writer.Write(DataFile1);
                writer.Write(DataFile2);
                writer.Write(OverlapThreshold);
                writer.Write(CreateOutput);
                writer.Write(OutputFile);
                writer.Write(CreateOutputOnlyOnCondition);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI31735",
                    "Unable to write create redacted text settings.", ex);
            }
        }

        #endregion Methods
    }
}
