using System;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using System.IO;
using Extract.FileActionManager.Forms;
using Extract.SqlDatabase;
using Extract.AttributeFinder;
using Extract.DataEntry;

namespace Extract.FileActionManager.FileProcessors.Utilities
{
    internal class ExpandTextHelper
    {
        /// <summary>
        /// Regex that parses text to find "matches" where each match is a section of the source
        /// text that alternates between recognized queries and non-query text. The sum of all
        /// matches = the original source text.
        /// </summary>
        static readonly Regex _queryParserRegex =
            new(@"((?!<Query>[\s\S]+?</Query>)[\S\s])+|<Query>[\s\S]+?</Query>",
                RegexOptions.Compiled);

        /// <summary>
        /// Regex that finds all shorthand attribute queries in text.
        /// </summary>
        static readonly Regex _attributeQueryFinderRegex = new(@"</[\s\S]+?>", RegexOptions.Compiled);

        /// <summary>
        /// Indicates that settings have been changed, but not saved.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Indicates whether data to run data entry queries has been initialized for the current
        /// file.
        /// </summary>
        bool _queryDataInitialized;

        /// <summary>
        /// The <see cref="DbConnection"/> to use to resolve data queries.
        /// </summary>
        DbConnection _dbConnection;

        /// <summary>
        /// The name of the VOA file that should be used to expand any attribute queries.
        /// </summary>
        string _dataFileName = "<SourceDocName>.voa";

        /// <summary>
        /// Gets or sets the name of the VOA file to use for attribute value expansion.
        /// </summary>
        /// <value>
        /// The name of the VOA file to use for attribute value expansion.
        /// </value>
        public string DataFileName
        {
            get
            {
                return _dataFileName;
            }

            set
            {
                _dirty |= !string.Equals(_dataFileName, value, StringComparison.Ordinal);
                _dataFileName = value;
            }
        }

        /// <summary>
        /// Indicates whether the VOA file was loaded.
        /// </summary>
        bool _dataFileLoaded;

        /// <summary>
        /// The last loaded file name.
        /// </summary>
        string loadedFileName = string.Empty;

        /// <summary>
        /// Expand all path tags/functions and data queries in the specified <see paramref="text"/>.
        /// <para><b>Note</b></para>
        /// This expansion supports shorthand attribute queries in the form &lt;/AttributeName&gt;
        /// </summary>
        /// <param name="text">The text to be expanded.</param>
        /// <param name="fileRecord">The <see cref="FileRecord"/> relating to the text to be
        /// expanded.</param>
        /// <param name="pathTags">The <see cref="FileActionManagerPathTags"/> instance to use to
        /// expand path tags and functions in the <see paramref="text"/>.</param>
        /// <param name="fileProcessingDB">The File Action Manager database being used for
        /// processing.</param>
        /// <returns><see paramref="text"/> with all path tags/functions as well as data queries
        /// expanded.</returns>
        public string ExpandText(string text, FileRecord fileRecord, FileActionManagerPathTags pathTags,
            IFileProcessingDB fileProcessingDB)
        {
            try
            {
                if (!fileRecord.Name.Equals(this.loadedFileName))
                {
                    _dataFileLoaded = false;
                    _queryDataInitialized = false;
                    this.loadedFileName = fileRecord.Name;
                }

                // Don't attempt to expand a blank string.
                if (string.IsNullOrWhiteSpace(text))
                {
                    return "";
                }

                string expandedOutput = "";

                // Parse the source text into alternating "matches" where every other "match" is a
                // query and the "matches" in-between are non-query text.
                var matches = _queryParserRegex.Matches(text)
                    .OfType<Match>()
                    .ToList();

                // Iterate all non-query text to see if it contains any shorthand query syntax that
                // needs to be expanded.
                // (</AttributeName> for <Query><Attribute>AttributeName</Attribute></Query>)
                foreach (Match match in matches
                    .Where(match => !IsQuery(match))
                    .ToArray())
                {
                    // Substitute any attribute query shorthand with the full query syntax.
                    string matchText =
                        _attributeQueryFinderRegex.Replace(match.Value, SubstituteAttributeQuery);

                    // If after substitutions the _queryParserRegex finds more than one partition, or
                    // the one and only partition is a query, one or more shorthand queries were
                    // expanded. Insert the expanded partitions in place of the original one.
                    var subMatches = _queryParserRegex.Matches(matchText);
                    if (subMatches.Count > 1 || IsQuery(subMatches[0]))
                    {
                        int index = matches.IndexOf(match);
                        matches.RemoveAt(index);
                        matches.InsertRange(index, subMatches.OfType<Match>());
                    }
                }

                // Iterate all partitions of the source text, evaluating any queries as we go.
                foreach (Match match in matches)
                {
                    if (IsQuery(match))
                    {
                        // The first time a query in encountered, load the database and data for all
                        // subsequent queries for this files to use.
                        if (!_queryDataInitialized)
                        {
                            if (fileProcessingDB != null)
                            {
                                var connectionString = SqlUtil.CreateConnectionString(fileProcessingDB.DatabaseServer, fileProcessingDB.DatabaseName);
                                _dbConnection = new ExtractRoleConnection(connectionString);
                                _dbConnection.Open();
                            }

                            IUnknownVector sourceAttributes = new();
                            string dataFileName = pathTags.Expand(DataFileName);
                            if (File.Exists(dataFileName))
                            {
                                // If data file exists, load it.
                                sourceAttributes.LoadFrom(dataFileName, false);

                                // So that the garbage collector knows of and properly manages the associated
                                // memory.
                                sourceAttributes.ReportMemoryUsage();

                                _dataFileLoaded = true;
                            }

                            AttributeStatusInfo.InitializeForQuery(sourceAttributes,
                                fileRecord.Name, _dbConnection, pathTags);

                            _queryDataInitialized = true;
                        }

                        // If data file does not exist and query appears to contain an attribute
                        // query, note the issue for later logging.
                        if (!_dataFileLoaded && match.Value.IndexOf(
                                "<Attribute", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            throw new ExtractException("ELI54296", "Data file is missing.");
                        }

                        // If the database connection does not exist and query appears to contain an
                        // SQL query, note the issue for later logging.
                        if (_dbConnection == null && match.Value.IndexOf(
                                "<SQL", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            throw new ExtractException("ELI54294", "Database connection is missing.");
                        }

                        try
                        {
                            // Append the query result to the expanded output in place of the query.
                            using var dataQuery = DataEntryQuery.Create(match.Value, null, _dbConnection);
                            expandedOutput += string.Join("\r\n", dataQuery.Evaluate().ToStringArray());
                        }
                        catch (Exception ex)
                        {
                            expandedOutput += "<Unable to evaluate query>";
                            var ee = new ExtractException("ELI54298",
                                "Unable to expand data query in email", ex);
                            ee.AddDebugData("Query", match.Value, false);
                            ee.AddDebugData("SourceDocName", fileRecord.Name, false);
                            ee.AddDebugData("FPS",
                                pathTags.Expand(FileActionManagerPathTags.FpsFileNameTag), false);
                            ee.Log();
                        }
                    }
                    else
                    {
                        // If the database connection does not exist and the text appears to contain
                        // tags than need it, note the issue for later logging.
                        if (fileProcessingDB == null &&
                            (match.Value.IndexOf(FileActionManagerPathTags.DatabaseActionTag, StringComparison.OrdinalIgnoreCase) >= 0 ||
                             match.Value.IndexOf(FileActionManagerPathTags.DatabaseServerTag, StringComparison.OrdinalIgnoreCase) >= 0 ||
                             match.Value.IndexOf(FileActionManagerPathTags.DatabaseServerTag, StringComparison.OrdinalIgnoreCase) >= 0))

                        {
                            throw new ExtractException("ELI54295", "Database connection is missing.");
                        }

                        // Append any non-query text as is.
                        expandedOutput += match.Value;
                    }
                }

                // Once all queries have been expanded, expand any path tags and functions as well.
                return pathTags.Expand(expandedOutput);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI54297");
            }
        }

        /// <summary>
        /// Determines whether the specified <see paramref="match"/> is a data query.
        /// </summary>
        /// <param name="match">The <see cref="Match"/> to check.</param>
        /// <returns><see langword="true"/> if the match is a data query; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        static bool IsQuery(Match match)
        {
            return match.Value.StartsWith("<Query>", StringComparison.OrdinalIgnoreCase) &&
                   match.Value.EndsWith("</Query>", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Substitutes full data query syntax for any shorthand attribute queries within the
        /// specified <see paramref="match"/>.
        /// </summary>
        /// <param name="match">The <see cref="Match"/> for which substitution should be done.
        /// </param>
        /// <returns>The text of the match with full data query syntax substituted for any shorthand
        /// attribute queries </returns>
        static string SubstituteAttributeQuery(Match match)
        {
            return "<Query><Attribute>" +
                match.Value.Substring(1, match.Length - 2) +
                "</Attribute></Query>";
        }
    }
}
