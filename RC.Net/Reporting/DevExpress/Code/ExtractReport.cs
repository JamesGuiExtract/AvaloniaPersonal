using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using Extract.Reporting;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Extract.ReportingDevExpress
{
    /// <summary>
    /// Represents a report class that contains a <see cref="ReportDocument"/> and the associated
    /// database connection and parameter information.
    /// </summary>
    public class ExtractReport : IDisposable, IExtractReport
    {
        #region Constants

        /// <summary>
        /// The current version of the <see cref="ExtractReport"/> object.
        /// </summary>
        static readonly int _VERSION = 3;

        /// <summary>
        /// The root node of the XML data contained in the parameter file associated
        /// with the report file.
        /// </summary>
        static readonly string _ROOT_NODE_NAME = "ExtractReportParameterData";

        /// <summary>
        /// The parameters node of the XML data contained in the parameter file
        /// associated with the report file.
        /// </summary>
        static readonly string _PARAMETERS_NODE_NAME = "ReportParameters";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name of the server to connect to.
        /// </summary>
        string _serverName;

        /// <summary>
        /// The name of the database to connect to.
        /// </summary>
        string _databaseName;

        /// <summary>
        /// The name of the workflow to report on
        /// </summary>
        string _workflowName;

        /// <summary>
        /// The name of the report file.
        /// </summary>
        string _reportFileName;

        /// <summary>
        /// The <see cref="XtraReport"/> for this <see cref="ExtractReport"/> instance.
        /// </summary>
        XtraReport _report;

        /// <summary>
        /// The collection of parameters for this report.
        /// </summary>
        Dictionary<string, IExtractReportParameter> _parameters =
            new Dictionary<string, IExtractReportParameter>();

        /// <summary>
        /// Temporarily stores the parameters chosen by the user when prompted so that they can be
        /// used when refreshing the report.
        /// </summary>
        string _activeParametersXml;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ExtractReport"/> class.
        /// </summary>
        /// <param name="serverName">The server to connect to.</param>
        /// <param name="databaseName">The database to connect to.</param>
        /// <param name="workflowName">The workflow to report on.</param>
        /// <param name="fileName">The name of the report file to load.</param>
        /// <param name="promptForParameters">Whether to prompt the user to
        /// enter in new values for the parameters or to use the values
        /// stored in the XML file.</param>
        public ExtractReport(string serverName, string databaseName, string workflowName,
            string fileName)
        {
            try
            {
                // Ensure that a valid file has been specified
                ExtractException.Assert("ELI23716", "File name cannot be null or empty!",
                    !string.IsNullOrEmpty(fileName));
                ExtractException.Assert("ELI23717", "File does not exist!",
                    File.Exists(fileName), "Report File Name", fileName);

                _report = XtraReport.FromFile(fileName);
                _serverName = serverName;
                _databaseName = databaseName;
                _workflowName = workflowName;
                _reportFileName = fileName;

                SetDatabaseConnection();
            }
            catch (Exception ex)
            {
                var ee = ExtractException.AsExtractException("ELI23718", ex);
                ee.AddDebugData("Server name", serverName, false);
                ee.AddDebugData("Database name", databaseName, false);
                ee.AddDebugData("Workflow name", workflowName, false);
                ee.AddDebugData("FileName", fileName, false);
                throw ee;
            }
        }

        /// <overloads>Initializes a new <see cref="ExtractReport"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="ExtractReport"/> class without a database.
        /// <para><b>Note</b></para>
        /// If this constructor is used, <see cref="Initialize"/> must be called against the target
        /// database before the report can be run.
        /// </summary>
        /// <param name="fileName">The name of the report file to load.</param>
        public ExtractReport(string fileName)
        {
            try
            {
                // Ensure that a valid file has been specified
                ExtractException.Assert("ELI40291", "File name cannot be null or empty!",
                    !string.IsNullOrEmpty(fileName));
                ExtractException.Assert("ELI40292", "File does not exist!",
                    File.Exists(fileName), "Report File Name", fileName);

                _reportFileName = fileName;

                _report = XtraReport.FromFile(fileName);
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI36066");
                ee.AddDebugData("File name", fileName, false);
                throw ee;
            }
        }

        public ExtractReport(ExtractReport extractReport)
        {
            try
            {
                _report = XtraReport.FromFile(extractReport._reportFileName);
                _serverName = extractReport._serverName;
                _databaseName = extractReport._databaseName;
                _workflowName = extractReport._workflowName;
                _reportFileName = extractReport._reportFileName;

                SetDatabaseConnection();

                _parameters.AddRange(extractReport._parameters);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49914");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the database server name.
        /// </summary>
        /// <value>
        /// The database server name.
        /// </value>
        public string DatabaseServer
        {
            get
            {
                return _serverName;
            }
        }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        public string DatabaseName
        {
            get
            {
                return _databaseName;
            }
        }

        /// <summary>
        /// Gets the name of the workflow.
        /// </summary>
        /// <value>
        /// The name of the workflow.
        /// </value>
        public string WorkflowName
        {
            get
            {
                return _workflowName;
            }
        }

        /// <summary>
        /// Gets the report file name.
        /// </summary>
        /// <returns>The report file name.</returns>
        public string FileName
        {
            get
            {
                return _reportFileName;
            }
        }

        /// <summary>
        /// Gets the <see cref="ReportDocument"/>.
        /// </summary>
        /// <returns>The <see cref="ReportDocument"/>.</returns>
        public object ReportDocument
        {
            get
            {
                return _report;
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="ExtractReportParameter{T}"/> objects.
        /// </summary>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of
        /// <see cref="ExtractReportParameter{T}"/> objects.</returns>
        public Dictionary<string, IExtractReportParameter> ParametersCollection
        {
            get
            {
                return _parameters;
            }
        }

        /// <summary>
        /// Gets the absolute path to the saved report folder (will not end in '\').
        /// </summary>
        /// <returns>The absolute path to the saved report folder (will not end in '\').</returns>
        public static string SavedReportFolder
        {
            get
            {
                try
                {
                    // Ensure the directory exists, if not create it
                    if (!Directory.Exists(ExtractReportUtils.SavedReportFolder))
                    {
                        Directory.CreateDirectory(ExtractReportUtils.SavedReportFolder);
                    }

                    return ExtractReportUtils.SavedReportFolder;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI23894", ex);
                }
            }
        }

        /// <summary>
        /// Gets the absolute path to the standard report folder (will not end in '\').
        /// </summary>
        /// <returns>The absolute path to the standard report folder (will not end in '\').</returns>
        public static string StandardReportFolder
        {
            get
            {
                try
                {
                    // Ensure the directory exists, if not create it
                    if (!Directory.Exists(ExtractReportUtils.StandardReportFolder))
                    {
                        Directory.CreateDirectory(ExtractReportUtils.StandardReportFolder);
                    }

                    return ExtractReportUtils.StandardReportFolder;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI23895", ex);
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Checks if a parameter exists
        /// </summary>
        /// <param name="parameterName">The parameter to check</param>
        /// <param name="withDefault">If true the parameter must have a default values, if false the must be an empty default value</param>
        /// <returns>false if the parameter does not exist, true if parameter exists and meets the conditions withDefault</returns>
        public bool ParameterExists(string parameterName, bool withDefault)
        {
            string fileName = ComputeXmlFileName();

            // if the Extract parameter collection is empty there was no xml file so no defaults
            // for the parameter names need to look in the loaded XtraReport for the names
            if (!File.Exists(fileName))
            {
                if (withDefault)
                    return false;

                foreach (var p in _report?.Parameters)
                {
                    if (p.Name == parameterName)
                        return true;
                }
            }
            else
            {
                // This is read directly from the xml because loading the parameters from the XML could require the
                // database server and name to have been set, which is not always the case when this is called.
                XDocument parameterFile = XDocument.Load(fileName);
                var parameterNameElement = parameterFile
                    .XPathSelectElement($"/ExtractReportParameterData/ReportParameters/*[@Name=\"{parameterName}\"]");

                if (parameterNameElement is null)
                    return false;

                var defaultValue = parameterNameElement.Attribute("Default").Value;
                return withDefault == !string.IsNullOrWhiteSpace(defaultValue);
            }

            return false;
        }

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
        public void ExportReportToFile(string fileName, bool overwrite)
        {
            try
            {
                ExtractException.Assert("ELI23719", "File name cannot be null or empty string!",
                    !string.IsNullOrEmpty(fileName));

                if (File.Exists(fileName))
                {
                    if (overwrite)
                    {
                        FileSystemMethods.DeleteFile(fileName, false);
                    }
                    else
                    {
                        ExtractException ee = new ExtractException("ELI23720",
                            "Cannot export report to existing file!");
                        ee.AddDebugData("Export File Name", fileName, false);
                        ee.AddDebugData("Report File", _reportFileName, false);
                        throw ee;
                    }
                }

                string extension = Path.GetExtension(fileName);

                if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    _report.ExportToPdf(fileName);
                }
                else if (extension.Equals(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    _report.ExportToXls(fileName);
                }
                else if  (extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    _report.ExportToXlsx(fileName);
                }
                else if (extension.Equals(".html", StringComparison.OrdinalIgnoreCase))
                {
                    _report.ExportToHtml(fileName);
                }
                else
                {
                    ExtractException ee = new ExtractException("ELI23721",
                        "Unrecognized file extension for report exporting!");
                    ee.AddDebugData("Export File Name", fileName, false);
                    ee.AddDebugData("Export File Extension", Path.GetExtension(fileName), false);
                    ee.AddDebugData("Report File", _reportFileName, false);
                    throw ee;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23722", ex);
            }
        }

        /// <summary>
        /// Initializes the report against the specified database.
        /// </summary>
        /// <param name="serverName">The server to connect to.</param>
        /// <param name="databaseName">The database to connect to.</param>
        /// <param name="workflowName">The workflow to report on.</param>
        public void Initialize(string serverName, string databaseName, string workflowName)
        {
            try
            {
                if (_report == null)
                {
                    _report = XtraReport.FromFile(_reportFileName);
                }

                _serverName = serverName;
                _databaseName = databaseName;
                _workflowName = workflowName;

                SetDatabaseConnection();

                ParseParameterXml(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36067");
            }
        }

        /// <summary>
        /// Refreshes the report so that it reflects up-to-date data in the database.
        /// </summary>
        public void Refresh()
        {
            try
            {
                SetDatabaseConnection();
                SetParameters(false, true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34275");
            }
        }

        /// <summary>
        /// Sets the database connection for the report document object.
        /// </summary>
        void SetDatabaseConnection()
        {
            var sqlConnection = _report.DataSource as SqlDataSource;
            var connectionParameters = sqlConnection?.ConnectionParameters as MsSqlConnectionParameters;
            if (connectionParameters != null)
            {
                connectionParameters.DatabaseName = _databaseName;
                connectionParameters.ServerName = _serverName;
                connectionParameters.AuthorizationType = MsSqlAuthorizationType.Windows;
            }
        }

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
        public bool SetParameters(bool promptForParameters, bool isRefresh)
        {
            try
            {
                ParseParameterXml(isRefresh);

                // Check if prompting
                if (promptForParameters)
                {
                    // Display the parameter prompt
                    if (!DisplayParameterPrompt())
                    {
                        return false;
                    }
                }
                // Not prompting, need to check for missing parameter values
                else if (MissingParameterValue())
                {
                    // Not prompting, throw an exception
                    ExtractException ee = new ExtractException("ELI23723",
                        "Parameter values missing in the XML file!");
                    ee.AddDebugData("XML File Name", ComputeXmlFileName(), false);
                    throw ee;
                }

                // Get the collection of parameters
                ParameterCollection reportParameters = _report.Parameters;

                // Count of parameters that have been set
                int numberOfParametersSet = 0;
                foreach (IExtractReportParameter parameter in _parameters.Values)
                {
                    if (parameter is TextParameter text)
                    {
                        numberOfParametersSet +=
                            SetParameterValues(reportParameters, text.ParameterName,
                            text.ParameterValue);
                        continue;
                    }

                    if (parameter is NumberParameter number)
                    {
                        numberOfParametersSet += SetParameterValues(reportParameters,
                            number.ParameterName, number.ParameterValue);
                        continue;
                    }

                    if (parameter is DateParameter date)
                    {
                        numberOfParametersSet += SetParameterValues(reportParameters,
                            date.ParameterName, date.ParameterValue);
                        continue;
                    }

                    if (parameter is DateRangeParameter dateRange)
                    {
                        var dr = new Range<DateTime>(dateRange.Minimum, dateRange.Maximum);
                        numberOfParametersSet += SetParameterValues(reportParameters, dateRange.ParameterName, dr);

                        continue;
                    }

                    if (parameter is ValueListParameter valueList)
                    {
                        numberOfParametersSet += SetParameterValues(reportParameters,
                            valueList.ParameterName, valueList.ParameterValue);
                        continue;
                    }

                    // If reached this point the parameter was an unrecognized type, throw an exception
                    ExtractException ee = new ExtractException("ELI23724",
                        "Unrecognized parameter type!");
                    ee.AddDebugData("Parameter Name",
                        parameter != null ? parameter.ParameterName : "null", false);
                    ee.AddDebugData("Parameter Type",
                        parameter != null ? parameter.GetType().ToString() : "null", false);
                    throw ee;
                }

                // Set all Extract Systems parameters
                numberOfParametersSet += SetParameterValues(reportParameters,
                    "ES_Username", Environment.UserName, false);
                numberOfParametersSet += SetParameterValues(reportParameters,
                    "ES_DatabaseName", _databaseName, false);
                numberOfParametersSet += SetParameterValues(reportParameters,
                    "ES_ServerName", _serverName, false);
                numberOfParametersSet += SetParameterValues(reportParameters,
                    "ES_WorkflowName", _workflowName, false);

                // Get the count of "non-linked" parameters
                int numberOfParameters = GetNonLinkedParameterCount(reportParameters);

                // Ensure that all non-linked parameters have been set
                ExtractException.Assert("ELI23850",
                    "Report contains parameters not defined in the XML file!",
                    numberOfParameters == numberOfParametersSet,
                    "Report Parameter Count", reportParameters.Count,
                    "Parameters Set Count", numberOfParametersSet);

                // If the user entered parameters, keep track of which parameters they chose so they
                // can be used if the report is refreshed.
                if (promptForParameters)
                {
                    _activeParametersXml = GetXml();
                }

                return true;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25075", ex);
                ee.AddDebugData("Report File Name", _reportFileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Parses the parameter XML.
        /// </summary>
        /// <param name="isRefresh"><see langword="true"/> parameters are being set for a report
        /// refresh, <see langword="false"/> if they are being set for the initial load.</param>
        void ParseParameterXml(bool isRefresh)
        {
            // Show the wait cursor while parsing the parameter file
            using (Extract.Utilities.Forms.TemporaryWaitCursor waitCursor
                = new Extract.Utilities.Forms.TemporaryWaitCursor())
            {
                // If the user had previously specified parameters for this report, start with those.
                if (isRefresh && !string.IsNullOrEmpty(_activeParametersXml))
                {
                    ParseParameterXml(_activeParametersXml);
                }
                // Otherwise load defaults from the permanent XML file.
                else
                {
                    // Parse the parameter file
                    ParseParameterFile();
                }
            }
        }

        /// <overload>Sets all parameter fields that have the specified parameter name
        /// on the report (and any sub-reports).</overload>
        /// <summary>
        /// Sets the specified report parameter to the specified value.
        /// </summary>
        /// <param name="parameters">The collection of report parameters.</param>
        /// <param name="parameterName">The name of the parameter to set.</param>
        /// <param name="value">The value to set on the parameter.</param>
        /// <returns>The number of parameters that were set.</returns>
        /// <exception cref="ExtractException">If the specified parameter was not
        /// set on the report.</exception>
        int SetParameterValues(ParameterCollection parameters,
            string parameterName, object value)
        {
            return SetParameterValues(parameters, parameterName, value, true);
        }

        /// <summary>
        /// Sets the specified report parameter to the specified value.
        /// </summary>
        /// <param name="parameters">The collection of report parameters.</param>
        /// <param name="parameterName">The name of the parameter to set.</param>
        /// <param name="value">The value to set on the parameter.</param>
        /// <param name="exceptionIfNotSet">If <see langword="true"/> and the
        /// specified parameter is not set then an exception will be thrown.
        /// If <see langword="false"/> then no exception will be thrown and the
        /// return value will be 0.</param>
        /// <returns>The number of parameters that were set.</returns>
        int SetParameterValues(ParameterCollection parameters,
            string parameterName, object value, bool exceptionIfNotSet)
        {
            // Loop through all the parameters on the report looking for
            // parameters with a matching name.  Count each one that is set.
            int numSet = 0;
            foreach (var parameter in parameters)
            {
                // Only set non-linked parameters (linked parameters are automatically
                // set at run time).
                if (parameter.Name == parameterName)
                {
                    SetParameterValue(parameter, value);
                    numSet++;
                }
            }

            // If no parameters were set and exceptionIfNotSet is true, throw an exception
            if (numSet == 0 && exceptionIfNotSet)
            {
                ExtractException ee = new ExtractException("ELI23852",
                    "Specified parameter does not exist in the current report!");
                ee.AddDebugData("Parameter Name", parameterName, false);
                ee.AddDebugData("Report File", _reportFileName, false);
                throw ee;
            }

            return numSet;
        }

        /// <summary>
        /// Sets the specified report parameter to the specified value.
        /// </summary>
        /// <param name="parameter">The parameter to set.</param>
        /// <param name="value">The value to set on the parameter.</param>
        static void SetParameterValue(Parameter parameter, object value)
        {
            try
            {
                if (parameter.ValueSourceSettings is RangeParametersSettings rangeParameterSettings)
                {
                    if (parameter.Type == typeof(DateTime))
                    {
                        if (value is Range<DateTime> dateRangeValue)
                        {
                            // Range parameters have start and end parameters named in
                            rangeParameterSettings.StartParameter.Value = dateRangeValue.Start;
                            rangeParameterSettings.EndParameter.Value = dateRangeValue.End;
                        }
                    }
                }
                else
                {
                    if (parameter.MultiValue)
                    {
                        parameter.Value = ((string)value).Split(new char[] { ',', ' ' });
                    }
                    else
                        parameter.Value = value;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25076", ex);
                ee.AddDebugData("Parameter Name", parameter.Name, false);
                ee.AddDebugData("Parameter Value", value != null ? value.ToString() : "NULL", false);
                throw ee;
            }
        }

        /// <summary>
        /// Checks each of the parameters to see if a value has been set yet.
        /// </summary>
        /// <returns><see langword="true"/> if a parameter did not have its value
        /// set and <see langword="false"/> otherwise.</returns>
        bool MissingParameterValue()
        {
            foreach (IExtractReportParameter parameter in _parameters.Values)
            {
                if (!parameter.HasValueSet())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Parses the parameter XML file.
        /// </summary>
        /// <returns>If there was an xml file returns true if not returns false</returns>
        bool ParseParameterFile()
        {
            string xmlFileName = ComputeXmlFileName();

            // Check for file existence, if there is no xml file there are no
            // parameters to prompt for
            if (!File.Exists(xmlFileName))
            {
                LoadDefaultsFromReport();
                return false;
            }

            // Load the XML file into a string
            string xml = File.ReadAllText(xmlFileName, Encoding.ASCII);

            ParseParameterXml(xml);
            return true;
        }

        private void LoadDefaultsFromReport()
        {
            if (_report is null)
                return;

            foreach (var parameter in _report.Parameters)
            {
                if (parameter.Name.StartsWith("ES_"))
                    continue;

                var existingValue = GetExistingValue(parameter.Name);

                var extractParameter = CreateStaticValueListParameter(parameter, existingValue) ??
                    CreateDynamicValueListParameter(parameter, existingValue) ??
                    CreateDateRangeParameter(parameter, existingValue);

                if (extractParameter is null)
                {
                    if (parameter.Type == typeof(string))
                    {
                        extractParameter= new TextParameter(parameter.Name, (existingValue.Value ?? parameter.Value) as string);
                    }
                    else if (parameter.Type == typeof (DateTime))
                    {
                        extractParameter = new DateParameter(parameter.Name,
                                                                        existingValue.Value as DateTime? ?? DateTime.Now,
                                                                        true);
                    }
                    else if (parameter.Type.IsNumericType())
                    {
                        extractParameter = new NumberParameter(parameter.Name, (double)(existingValue.Value ?? parameter.Value));
                    }
                }
                if (extractParameter is null)
                {
                    ExtractException ee = new ExtractException("ELI50124", "Unable to create Extract report parameter.");
                    ee.AddDebugData("ReportParameter", parameter.Name);
                    throw ee;
                }
                _parameters[parameter.Name] = extractParameter;
            }
        }

        IExtractReportParameter CreateStaticValueListParameter(Parameter parameter, (object Value, Range<DateTime>? RangeValue) existingValue)
        {
            var staticSettings = parameter.ValueSourceSettings as StaticListLookUpSettings;
            if (staticSettings is null)
                return null;
            return new ValueListParameter( parameter.Name,
                staticSettings.LookUpValues
                    .Select(v => v.Value)
                    .OfType<string>(),
                (existingValue.Value ?? parameter.Value) as string,
                parameter.MultiValue);
        }

        IExtractReportParameter CreateDynamicValueListParameter(Parameter parameter, (object Value, Range<DateTime>? RangeValue) existingValue)
        {
            var dynamicSettings = parameter.ValueSourceSettings as DynamicListLookUpSettings;
            if (dynamicSettings is null)
                return null;

            var queryName = dynamicSettings.DataMember;
            var source = dynamicSettings.DataSource as SqlDataSource;
            if (source is null)
            {
                var ee = new ExtractException("ELI50108", "Invalid data source for lookup list");
                ee.AddDebugData("Parameter name", parameter.Name);
                throw ee;
            }
            var query = source.Queries[queryName] as SelectQuery;
            source.RebuildResultSchema();
            var queryString = query.GetSql(source.DBSchema);

            var valueList = new ValueListParameter(parameter.Name,
                                                   BuildValueListFromQuery(queryString),
                                                   (existingValue.Value ?? parameter.Value) as string,
                                                   parameter.MultiValue);
            valueList.ListQuery = queryString;
            return valueList;
        }

        IExtractReportParameter CreateDateRangeParameter(Parameter parameter, (object Value, Range<DateTime>? RangeValue) existingValue)
        {
            var rangeSettings = parameter.ValueSourceSettings as RangeParametersSettings;
            if (rangeSettings is null)
                return null;
            if (parameter.Type != typeof(DateTime))
            {
                var ee = new ExtractException("ELI50107", "Unknown range type.");
                ee.AddDebugData("Parameter name", parameter.Name);
                ee.AddDebugData("Parameter type", parameter.Type.FullName);
                throw ee;
            }

            var dateRangeParameter = new DateRangeParameter(parameter.Name, DateRangeValue.All);
            if (existingValue.Value != null)
            {
                dateRangeParameter.Value = existingValue.Value;
                if (existingValue.Value as DateRangeValue? == DateRangeValue.Custom)
                {
                    dateRangeParameter.Maximum = existingValue.RangeValue?.End ?? DateTime.Now;
                    dateRangeParameter.Minimum = existingValue.RangeValue?.Start ?? DateTime.Now;
                }
            }
            return dateRangeParameter;
        }

        /// <summary>
        /// Parses the parameter XML.
        /// </summary>
        /// <param name="xml"></param>
        void ParseParameterXml(string xml)
        {
            // Parse the XML file
            using (StringReader reader = new StringReader(xml))
            {
                XmlTextReader xmlReader = new XmlTextReader(reader);
                xmlReader.WhitespaceHandling = WhitespaceHandling.Significant;
                xmlReader.Normalization = true;
                xmlReader.Read();

                // Read the version number
                if (xmlReader.Name != _ROOT_NODE_NAME)
                {
                    ExtractException ee = new ExtractException("ELI23726",
                        "Invalid report parameter xml file!");
                    ee.AddDebugData("XML File Name", ComputeXmlFileName(), false);
                    throw ee;
                }

                int version = Convert.ToInt32(xmlReader.GetAttribute("Version"),
                    CultureInfo.InvariantCulture);
                if (version > _VERSION)
                {
                    ExtractException ee = new ExtractException("ELI23727",
                        "Invalid report parameter xml file version!");
                    ee.AddDebugData("Maximum version", _VERSION, false);
                    ee.AddDebugData("Version In File", version, false);
                    ee.AddDebugData("XML File Name", ComputeXmlFileName(), false);
                    throw ee;
                }

                // Check for parameters node
                xmlReader.Read();
                if (xmlReader.Name != _PARAMETERS_NODE_NAME)
                {
                    ExtractException ee = new ExtractException("ELI23728",
                        "Invalid report parameter xml file, no parameters node!");
                    ee.AddDebugData("XML File Name", ComputeXmlFileName(), false);
                    throw ee;
                }

                ReadParameters(xmlReader);
            }
        }

        /// <summary>
        /// Builds the name of the XML file related to the current report.
        /// </summary>
        /// <returns>The name of the XML file for the current report.</returns>
        string ComputeXmlFileName()
        {
            string xmlFileName = _reportFileName;
            xmlFileName = Path.GetDirectoryName(xmlFileName) + Path.DirectorySeparatorChar +
                Path.GetFileNameWithoutExtension(xmlFileName) + ".xml";
            return xmlFileName;
        }

        /// <summary>
        /// Gets the current value of the parameter with the name <paramref name="parameterName"/>
        /// </summary>
        /// <param name="parameterName">The name of the parameter to return the value of</param>
        /// <returns>Returns a tuple containing the value of the parameter and the Range Value if it is
        /// a date range parameter that has a custom value</returns>
        (object Value, Range<DateTime>? RangeValue) GetExistingValue(string parameterName)
        {
            object existingValue = null;
            Range<DateTime>? existingRange = null;
            
            if (ParametersCollection.TryGetValue(parameterName, out var existingParameter)
                && existingParameter.HasValueSet())
            {
                existingValue = existingParameter.Value;
                var dateRange = existingParameter as DateRangeParameter;
                if (dateRange != null && dateRange.ParameterValue == DateRangeValue.Custom)
                    existingRange = new Range<DateTime>(dateRange.Minimum, dateRange.Maximum);
            }
            return (existingValue, existingRange);
        }

        /// <summary>
        /// Reads the parameters from the specified <see cref="XmlTextReader"/>.
        /// </summary>
        /// <param name="xmlReader">The <see cref="XmlTextReader"/> to
        /// read the parameters from.</param>
        void ReadParameters(XmlTextReader xmlReader)
        {
            // Read the parameters
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    // Get the parameter type
                    string paramType = xmlReader.Name;

                    // Move to the name attribute and store it
                    xmlReader.MoveToAttribute("Name");
                    string paramName = xmlReader.Value;

                    string defaultVal = null;
                    var existingValue = GetExistingValue(paramName);

                    // Check whether the parameter already has a value. If so, it will be assigned
                    // in place of any default value.
                    if (existingValue.Value is null )
                    {
                        // Move to the default attribute and store it (ensure the default
                        // attribute exists)
                        xmlReader.MoveToAttribute("Default");
                        if (!xmlReader.Name.Equals("Default", StringComparison.OrdinalIgnoreCase))
                        {
                            ExtractException ee = new ExtractException("ELI23851",
                                "No 'Default' attribute for the current parameter!");
                            ee.AddDebugData("Parameter Type", paramType, false);
                            ee.AddDebugData("Parameter Name", paramName, false);
                            throw ee;
                        }
                        defaultVal = xmlReader.Value;
                    }

                    try
                    {
                        IExtractReportParameter param = null;
                        switch (paramType)
                        {
                            case "TextParameter":
                                if (string.IsNullOrEmpty(defaultVal))
                                {
                                    param = new TextParameter(paramName);
                                }
                                else
                                {
                                    param = new TextParameter(paramName, defaultVal);
                                }
                                break;

                            case "NumberParameter":
                                if (string.IsNullOrEmpty(defaultVal))
                                {
                                    param = new NumberParameter(paramName);
                                }
                                else
                                {
                                    double temp = Convert.ToDouble(defaultVal, CultureInfo.InvariantCulture);
                                    param = new NumberParameter(paramName, temp);
                                }
                                break;

                            case "DateParameter":
                                // Get whether to show the time (or just the date).
                                xmlReader.MoveToAttribute("ShowTime");
                                bool showTime = true;

                                if (xmlReader.Name.Equals("ShowTime", StringComparison.Ordinal))
                                {
                                    showTime = Convert.ToBoolean(xmlReader.Value, CultureInfo.InvariantCulture);
                                }

                                if (string.IsNullOrEmpty(defaultVal))
                                {
                                    param = new DateParameter(paramName, DateTime.Now, showTime);
                                }
                                else
                                {
                                    DateTime temp = Convert.ToDateTime(defaultVal, CultureInfo.InvariantCulture);

                                    param = new DateParameter(paramName, temp, showTime);
                                }
                                break;

                            case "DateRangeParameter":
                                if (string.IsNullOrEmpty(defaultVal))
                                {
                                    param = new DateRangeParameter(paramName);
                                }
                                else if (defaultVal == "Custom")
                                {
                                    // Get the min and max values
                                    xmlReader.MoveToAttribute("Min");
                                    ExtractException.Assert("ELI23855",
                                                            "Custom date range missing 'Min' attribute!",
                                                            xmlReader.Name.Equals("Min", StringComparison.Ordinal),
                                                            "Parameter Name",
                                                            paramName);
                                    DateTime min = Convert.ToDateTime(xmlReader.Value, CultureInfo.InvariantCulture);

                                    xmlReader.MoveToAttribute("Max");
                                    ExtractException.Assert("ELI23856",
                                                            "Custom date range missing 'Max' attribute!",
                                                            xmlReader.Name.Equals("Max", StringComparison.Ordinal),
                                                            "Parameter Name",
                                                            paramName);
                                    DateTime max = Convert.ToDateTime(xmlReader.Value, CultureInfo.InvariantCulture);

                                    param = new DateRangeParameter(paramName, min, max);
                                }
                                else
                                {
                                    DateRangeValue value = (DateRangeValue)Enum.Parse(typeof(DateRangeValue),
                                                                                      defaultVal,
                                                                                      true);
                                    param = new DateRangeParameter(paramName, value);
                                }

                                break;

                            case "ValueListParameter":
                                string[] values = null;
                                bool allowOtherValues = false;
                                bool multipleSelect = false;
                                string query = "";

                                // Check if the value list parameter is using a value list
                                // or a query
                                if (xmlReader.MoveToAttribute("Query"))
                                {
                                    query = xmlReader.Value;
                                    values = BuildValueListFromQuery(query);
                                }
                                else if (xmlReader.MoveToAttribute("Values"))
                                {
                                    // Get the list values
                                    values = xmlReader.Value
                                        .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                                    // Get the editable attribute
                                    ExtractException.Assert("ELI23858",
                                                            "Value list missing 'Editable' attribute!",
                                                            xmlReader.MoveToAttribute("Editable"),
                                                            "Parameter Name",
                                                            paramName);
                                    if (!bool.TryParse(xmlReader.Value, out allowOtherValues))
                                    {
                                        ExtractException ee1 = new ExtractException("ELI23729",
                                                                                   "Editable attribute has invalid value!");
                                        ee1.AddDebugData("Attribute Value", xmlReader.Value, false);
                                        throw ee1;
                                    }
                                }
                                else
                                {
                                    ExtractException ee2 = new ExtractException("ELI28200",
                                                                               "Value list must contain either a 'Values' or 'Query' setting.");
                                    ee2.AddDebugData("Parameter Name", paramName, false);
                                    throw ee2;
                                }

                                if (xmlReader.MoveToAttribute("MultipleSelect"))
                                {
                                    if (!bool.TryParse(xmlReader.Value, out multipleSelect))
                                    {
                                        ExtractException ee3 = new ExtractException("ELI50106",
                                                                                   "MultipleSelect attribute has invalid value!");
                                        ee3.AddDebugData("Attribute Value", xmlReader.Value, false);
                                        throw ee3;
                                    }
                                }

                                // If the report has not been initialized, allow the parameter
                                // value to be set without verification that it is a valid value
                                // for now. Validity of the value will be confirmed during the
                                // call to Initialize.
                                var valueList = new ValueListParameter(paramName,
                                                                       values,
                                                                       defaultVal as string,
                                                                       allowOtherValues || (_report == null),
                                                                       multipleSelect);
                                valueList.ListQuery = query;
                                param = valueList;
                                break;

                            default:
                                ExtractException ee4 = new ExtractException("ELI23730",
                                                                           "Unrecognized parameter type in XML file!");
                                ee4.AddDebugData("Parameter Type", xmlReader.Name, false);
                                throw ee4;
                        }

                        // If the parameter had an existing value, set it.
                        if (existingValue.Value != null)
                        {
                            param.Value = existingValue.Value;
                            var dateParameter = param as DateRangeParameter;
                            if (dateParameter != null && dateParameter.ParameterValue == DateRangeValue.Custom)
                            {
                                dateParameter.Minimum = existingValue.RangeValue.Value.Start;
                                dateParameter.Maximum = existingValue.RangeValue.Value.End;
                            }
                        }

                        // Add the parameter to the collection
                        _parameters[paramName] = param;
                    }
                    catch (Exception ex)
                    {
                        ExtractException ee =
                            ExtractException.AsExtractException("ELI23731", ex);
                        ee.AddDebugData("Parameter Name", paramName, false);
                        ee.AddDebugData("Parameter Value", defaultVal, false);
                        throw ee;
                    }
                }
            }
        }

        /// <summary>
        /// Builds the value list by running the specified query on the database and
        /// filling a string array with the results.
        /// </summary>
        /// <param name="query">The query to run.</param>
        /// <returns>A collection of strings.</returns>
        string[] BuildValueListFromQuery(string query)
        {
            // If the database connection has not yet been set, we cannot load the value list.
            if (string.IsNullOrWhiteSpace(_serverName) ||
                string.IsNullOrWhiteSpace(_databaseName))
            {
                return new string[] { "[No Values Found]" };
            }

            SqlConnection connection = null;
            SqlDataAdapter adapter = null;
            DataTable table = null;
            try
            {
                // Create a new table
                table = new DataTable();
                table.Locale = CultureInfo.InvariantCulture;

                // Open the connection
                connection = new SqlConnection("server=" + _serverName + ";database="
                    + _databaseName + ";connection timeout=30;Integrated Security=true");

                // Run the query and use it to fill a data table
                adapter = new SqlDataAdapter(query, connection);
                adapter.Fill(table);

                if (table.Rows.Count > 0)
                {
                    // Iterate through each row adding the first columns value to the list
                    List<string> values = new List<string>(table.Rows.Count);
                    foreach (DataRow row in table.Rows)
                    {
                        values.Add(row[0].ToString());
                    }

                    // Return the string array
                    return values.ToArray();
                }
                else
                {
                    return new string[] { "[No Values Found]" };
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28201", ex);
                ee.AddDebugData("SQL Query", query, false);
                throw ee;
            }
            finally
            {
                // Ensure items are cleaned up
                if (table != null)
                {
                    table.Dispose();
                }
                if (adapter != null)
                {
                    adapter.Dispose();
                }
                if (connection != null)
                {
                    connection.Dispose();
                }
            }
        }

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
        public void WriteXmlFile(string fileName, bool overwrite)
        {
            XmlTextWriter xmlWriter = null;
            try
            {
                ExtractException.Assert("ELI23859", "File name cannot be null or empty string!",
                    !string.IsNullOrEmpty(fileName));

                // Check if the XML file exists
                if (File.Exists(fileName))
                {
                    // If overwrite was true then delete the file
                    if (overwrite)
                    {
                        FileSystemMethods.DeleteFile(fileName, false);
                    }
                    else
                    {
                        // Overwrite was false, throw an exception
                        ExtractException ee = new ExtractException("ELI23860",
                            "XML file already exists and overwrite was not specified!");
                        ee.AddDebugData("XML File Name", fileName, false);
                        throw ee;
                    }
                }

                // If there are no parameters to write, we are done.
                if (_parameters.Count == 0)
                {
                    return;
                }

                xmlWriter = new XmlTextWriter(fileName, Encoding.ASCII);
                xmlWriter.Formatting = System.Xml.Formatting.Indented;
                xmlWriter.Indentation = 4;

                // Write the root node
                xmlWriter.WriteStartElement(_ROOT_NODE_NAME);
                xmlWriter.WriteAttributeString("Version",
                    _VERSION.ToString(CultureInfo.InvariantCulture));

                // Write the parameters node
                xmlWriter.WriteStartElement(_PARAMETERS_NODE_NAME);

                // Write the parameters
                foreach (IExtractReportParameter parameter in _parameters.Values)
                {
                    parameter.WriteToXml(xmlWriter);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26503", ex);
                ee.AddDebugData("XML File To Write", fileName, false);
                ee.AddDebugData("Overwrite", overwrite, false);

                throw ee;
            }
            finally
            {
                if (xmlWriter != null)
                {
                    xmlWriter.Close();
                    xmlWriter = null;
                }
            }
        }

        /// <summary>
        /// Gets the parameters as a string in XML format.
        /// </summary>
        /// <returns>The XML string.</returns>
        string GetXml()
        {
            XmlWriter xmlWriter = null;
            try
            {
                StringBuilder xmlString = new StringBuilder();
                XmlWriterSettings xmlSettings = new XmlWriterSettings();
                xmlSettings.OmitXmlDeclaration = true;
                xmlWriter = XmlWriter.Create(xmlString, xmlSettings);

                // Write the root node
                xmlWriter.WriteStartElement(_ROOT_NODE_NAME);
                xmlWriter.WriteAttributeString("Version",
                    _VERSION.ToString(CultureInfo.InvariantCulture));

                // Write the parameters node
                xmlWriter.WriteStartElement(_PARAMETERS_NODE_NAME);

                // Write the parameters
                foreach (IExtractReportParameter parameter in _parameters.Values)
                {
                    parameter.WriteToXml(xmlWriter);
                }

                xmlWriter.Close();
                xmlWriter = null;

                return xmlString.ToString();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34318");
            }
            finally
            {
                if (xmlWriter != null)
                {
                    xmlWriter.Close();
                    xmlWriter = null;
                }
            }
        }

        /// <summary>
        /// Display the parameter entry form to the user.
        /// </summary>
        /// <returns><see langword="true"/> if the user clicked OK in parameter
        /// dialog and <see langword="false"/> if the user canceled.</returns>
        bool DisplayParameterPrompt()
        {
            // Ensure there are parameters to display
            if (_parameters.Count > 0)
            {
                using (ParameterEntryForm entryForm = new ParameterEntryForm(_parameters.Values))
                {
                    return entryForm.ShowDialog() == System.Windows.Forms.DialogResult.OK;
                }
            }
            else
            {
                // No parameters to display, just return true
                return true;
            }
        }

        /// <summary>
        /// Gets the count of "non-linked" parameters on the report.
        /// </summary>
        /// <param name="parameters">The collection of parameters to count.</param>
        /// <returns>The count of "non-linked" parameters on the report.</returns>
        static int GetNonLinkedParameterCount(ParameterCollection parameters)
        {
            try
            {
                // The Base DevExpress report ParameterCollection only includes the non linked parameters
                return parameters.Count;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25339", ex);
            }
        }

        #endregion Methods

        #region IDisposable Members

        /// <overloads>Releases all resources used by the <see cref="ExtractReport"/>.</overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="ExtractReport"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the <see cref="ExtractReport"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        virtual protected void Dispose(bool disposing)
        {
            // Dispose of managed objects
            if (disposing)
            {
                if (_parameters != null && _parameters.Count > 0)
                {
                    _parameters.Clear();
                    _parameters = null;
                }

                if (_report != null)
                {
                    _report.Dispose();
                    _report = null;
                }
            }

            // No unmanaged resources to dispose
        }

        #endregion
    }
}
