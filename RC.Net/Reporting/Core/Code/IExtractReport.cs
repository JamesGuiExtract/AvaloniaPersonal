using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Extract.Reporting
{
    public interface IExtractReport : IDisposable
    {
        /// <summary>
        /// Exports the report object to the specified file.
        /// <para><b>Note:</b></para>
        /// The export format is determined from the file extension of <paramref name="fileName"/>.
        /// If the extension is '.pdf' then the output format will be pdf if the extension is
        /// '.xls' then the output format will be Excel.  If the extension is anything else then
        /// an exception will be thrown.  If the output file already exists and
        /// <paramref name="overwrite"/> is <see langword="false"/> then an exception will be
        /// thrown.
        /// </summary>
        /// <param name="fileName">The fully qualified path to the file that the report
        /// will be exported to. This value must not be <see langword="null"/> or empty string.
        /// If the file exists and <paramref name="overwrite"/>
        /// is <see langword="false"/> an exception will be thrown.  If <paramref name="overwrite"/>
        /// is <see langword="true"/> and the file exists it will be overwritten.</param>
        /// <param name="overwrite">if <see langword="true"/> and <paramref name="fileName"/>
        /// exists it will be overwritten; if <see langword="false"/> and
        /// <paramref name="fileName"/> exists then an exception will be thrown.</param>
        /// <exception cref="ExtractException">If <paramref name="fileName"/> exists and
        /// <paramref name="overwrite"/> is <see langword="false"/>.</exception>
        /// <exception cref="ExtractException">If the file extension of <paramref name="fileName"/>
        /// is not '.pdf' or '.xls'.</exception>
        void ExportReportToFile(string fileName, bool overwrite);

        /// <summary>
        /// Initializes the report against the specified database.
        /// </summary>
        /// <param name="serverName">The server to connect to.</param>
        /// <param name="databaseName">The database to connect to.</param>
        /// <param name="workflowName">The workflow to report on.</param>
        void Initialize(string serverName, string databaseName, string workflowName);

        /// <summary>
        /// Refreshes the report so that it reflects up-to-date data in the database.
        /// </summary>
        void Refresh();

        /// <summary>
        /// Writes the parameters to a specified XML file.
        /// </summary>
        /// <param name="fileName">The name of the file to write the XML to.
        /// Must not be <see langword="null"/> or empty.</param>
        /// <param name="overwrite">If <see langword="true"/> then will overwrite
        /// <paramref name="fileName"/> if it exists; if <see langword="false"/>
        /// will throw an exception if <paramref name="fileName"/> exists.</param>
        /// <exception cref="ExtractException">If <paramref name="fileName"/>
        /// is <see langword="null"/> or empty string.</exception>
        /// <exception cref="ExtractException">If <paramref name="fileName"/>
        /// exists and <paramref name="overwrite"/> is <see langword="false"/>.</exception>
        void WriteXmlFile(string fileName, bool overwrite);

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        string DatabaseName { get; }
        /// <summary>
        /// Gets the database server name.
        /// </summary>
        /// <value>
        /// The database server name.
        /// </value>
        string DatabaseServer { get; }
        /// <summary>
        /// Gets the report file name.
        /// </summary>
        /// <returns>The report file name.</returns>
        string FileName { get; }
        /// <summary>
        /// Gets the collection of <see cref="ExtractReportParameter{T}"/> objects.
        /// </summary>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of
        /// <see cref="ExtractReportParameter{T}"/> objects.</returns>
        Dictionary<string, IExtractReportParameter> ParametersCollection { get; }
        /// <summary>
        /// Gets the <see cref="ReportDocument"/>.
        /// </summary>
        /// <returns>The <see cref="ReportDocument"/>.</returns>
        object ReportDocument { get; }
        /// <summary>
        /// Gets the name of the workflow.
        /// </summary>
        /// <value>
        /// The name of the workflow.
        /// </value>
        string WorkflowName { get; }

        /// <summary>
        /// Checks if a parameter exists 
        /// </summary>
        /// <param name="parameterName">The parameter to check</param>
        /// <param name="withDefault">If true the parameter must have a default values, if false the must be an empty default value</param>
        /// <returns>false if the parameter does not exist, true if parameter exists and meets the conditions withDefault</returns>
        bool ParameterExists(string parameterName, bool withDefault);

        /// <summary>
        /// Sets the parameters.  If <paramref name="promptForParameters"/> is <see langword="true"/> then
        /// will display a prompt to enter in new values for the parameters, otherwise
        /// if all the values have been specified in the XML then will just load the defaults
        /// from the XML file, if no XML file exists parameters are generated from the report definition with defaults
        /// that are in the definition
        /// </summary>
        /// <param name="promptForParameters">Whether to force a prompt for parameter values
        /// or not.</param>
        /// <param name="isRefresh"><see langword="true"/> parameters are being set for a report
        /// refresh, <see langword="false"/> if they are being set for the initial load.</param>
        /// <returns><see langword="true"/> if parameters have been set and
        /// <see langword="false"/> otherwise.</returns>
        bool SetParameters(bool promptForParameters, bool isRefresh);
    }
}
