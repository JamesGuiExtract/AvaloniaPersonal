using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Extract.ReportViewer
{
    /// <summary>
    /// Represents a report class that contains a <see cref="ReportDocument"/> and the associated
    /// database connection and parameter information.
    /// </summary>
    public class ExtractReport : IDisposable
    {
        #region Constants

        /// <summary>
        /// Relative path to the reports folder (relative to the current applications directory).
        /// </summary>
        private static readonly string _REPORT_FOLDER_PATH =
            Path.Combine(FileSystemMethods.CommonApplicationDataPath, "Reports");

        /// <summary>
        /// Folder which contains the standard reports
        /// </summary>
        private static readonly string _STANDARD_REPORT_FOLDER =
            Path.Combine(_REPORT_FOLDER_PATH, "Standard reports");

        /// <summary>
        /// Folder which contains the saved reports
        /// </summary>
        private static readonly string _SAVED_REPORT_FOLDER =
            Path.Combine(_REPORT_FOLDER_PATH, "Saved reports");

        /// <summary>
        /// The current version of the <see cref="ExtractReport"/> object.
        /// </summary>
        private static readonly int _VERSION = 2;

        /// <summary>
        /// The root node of the XML data contained in the parameter file associated
        /// with the report file.
        /// </summary>
        private static readonly string _ROOT_NODE_NAME = "ExtractReportParameterData";

        /// <summary>
        /// The parameters node of the XML data contained in the parameter file
        /// associated with the report file.
        /// </summary>
        private static readonly string _PARAMETERS_NODE_NAME = "ReportParameters";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Indicates whether the parameters have all been set.
        /// </summary>
        bool _canceledInitialization;

        /// <summary>
        /// The name of the server to connect to.
        /// </summary>
        string _serverName;

        /// <summary>
        /// The name of the database to connect to.
        /// </summary>
        string _databaseName;

        /// <summary>
        /// The name of the report file.
        /// </summary>
        string _reportFileName;

        /// <summary>
        /// The <see cref="ReportDocument"/> for this <see cref="ExtractReport"/> instance.
        /// </summary>
        ReportDocument _report = new ReportDocument();

        /// <summary>
        /// The collection of parameters for this report.
        /// </summary>
        Dictionary<string, IExtractReportParameter> _parameters =
            new Dictionary<string, IExtractReportParameter>();

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new <see cref="ExtractReport"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="ExtractReport"/> class.
        /// </summary>
        /// <param name="serverName">The server to connect to.</param>
        /// <param name="databaseName">The database to connect to.</param>
        /// <param name="fileName">The name of the report file to load.</param>
        public ExtractReport(string serverName, string databaseName, string fileName)
            : this(serverName, databaseName, fileName, true)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ExtractReport"/> class.
        /// </summary>
        /// <param name="serverName">The server to connect to.</param>
        /// <param name="databaseName">The database to connect to.</param>
        /// <param name="fileName">The name of the report file to load.</param>
        /// <param name="promptForParameters">Whether to prompt the user to
        /// enter in new values for the parameters or to use the values
        /// stored in the XML file.</param>
        public ExtractReport(string serverName, string databaseName, string fileName,
            bool promptForParameters)
        {
            try
            {
                // Ensure that a valid file has been specified
                ExtractException.Assert("ELI23716", "File name cannot be null or empty!",
                    !string.IsNullOrEmpty(fileName));
                ExtractException.Assert("ELI23717", "File does not exist!",
                    File.Exists(fileName), "Report File Name", fileName);

                _serverName = serverName;
                _databaseName = databaseName;
                _reportFileName = fileName;
                _report.FileName = fileName;

                SetDatabaseConnection();

                _canceledInitialization = !SetParameters(promptForParameters);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23718", ex);
            }
        }

        #endregion Constructors

        #region Properties

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
        public ReportDocument ReportDocument
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
        /// Gets whether the initialization was canceled or not (means the user
        /// hit cancel while entering parameters). If <see langword="true"/> the
        /// initialization was canceled; <see langword="false"/> otherwise.
        /// </summary>
        /// <returns><see langword="true"/> if intialization was canceled and
        /// <see langword="false"/> otherwise.</returns>
        public bool CanceledInitialization
        {
            get
            {
                return _canceledInitialization;
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
                    if (!Directory.Exists(_SAVED_REPORT_FOLDER))
                    {
                        Directory.CreateDirectory(_SAVED_REPORT_FOLDER);
                    }

                    return _SAVED_REPORT_FOLDER;
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
                    if (!Directory.Exists(_STANDARD_REPORT_FOLDER))
                    {
                        Directory.CreateDirectory(_STANDARD_REPORT_FOLDER);
                    }

                    return _STANDARD_REPORT_FOLDER;
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
        /// Exports the crystal report object to the specified file.
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
                    _report.ExportToDisk(ExportFormatType.PortableDocFormat, fileName);
                }
                else if (extension.Equals(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    _report.ExportToDisk(ExportFormatType.Excel, fileName);
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
        /// Sets the database connection for the report document object.
        /// </summary>
        private void SetDatabaseConnection()
        {
            // Set up the log on info for the report
            TableLogOnInfo logOnInfo = new TableLogOnInfo();
            logOnInfo.ConnectionInfo.ServerName = _serverName;
            logOnInfo.ConnectionInfo.DatabaseName = _databaseName;
            logOnInfo.ConnectionInfo.IntegratedSecurity = true;

            // Apply the log on info for all tables in the report
            foreach (Table table in _report.Database.Tables)
            {
                table.ApplyLogOnInfo(logOnInfo);
            }
        }

        /// <summary>
        /// Sets the parameters (after loading them from the XML file).  If
        /// <paramref name="promptForParameters"/> is <see langword="true"/> then
        /// will display a prompt to enter in new values for the parameters, otherwise
        /// if all the values have been specified in the XML then will just load the defaults
        /// from the XML file.
        /// </summary>
        /// <param name="promptForParameters">Whether to force a prompt for parameter values
        /// or not.</param>
        /// <returns><see langword="true"/> if parameters have been set and
        /// <see langword="false"/> otherwise.</returns>
        private bool SetParameters(bool promptForParameters)
        {
            try
            {
                // Show the wait cursor while parsing the parameter file
                using (Extract.Utilities.Forms.TemporaryWaitCursor waitCursor
                    = new Extract.Utilities.Forms.TemporaryWaitCursor())
                {
                    // Parse the parameter file
                    ParseParameterFile();
                }

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
                ParameterFieldDefinitions reportParameters = _report.DataDefinition.ParameterFields;

                // Count of parameters that have been set
                int numberOfParametersSet = 0;
                foreach (IExtractReportParameter parameter in _parameters.Values)
                {
                    TextParameter text = parameter as TextParameter;
                    if (text != null)
                    {
                        numberOfParametersSet +=
                            SetParameterValues(reportParameters, text.ParameterName,
                            text.ParameterValue);
                        continue;
                    }

                    NumberParameter number = parameter as NumberParameter;
                    if (number != null)
                    {
                        numberOfParametersSet += SetParameterValues(reportParameters,
                            number.ParameterName, number.ParameterValue);
                        continue;
                    }

                    DateParameter date = parameter as DateParameter;
                    if (date != null)
                    {
                        numberOfParametersSet += SetParameterValues(reportParameters,
                            date.ParameterName, date.ParameterValue);
                        continue;
                    }

                    DateRangeParameter dateRange = parameter as DateRangeParameter;
                    if (dateRange != null)
                    {
                        string minParam = dateRange.ParameterName + "_Min";
                        string maxParam = dateRange.ParameterName + "_Max";

                        numberOfParametersSet += SetParameterValues(reportParameters,
                            minParam, dateRange.Minimum);
                        numberOfParametersSet += SetParameterValues(reportParameters,
                            maxParam, dateRange.Maximum);
                        continue;
                    }

                    ValueListParameter valueList = parameter as ValueListParameter;
                    if (valueList != null)
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

                // Get the count of "non-linked" parameters
                int numberOfParameters = GetNonLinkedParameterCount(reportParameters);

                // Ensure that all non-linked parameters have been set
                ExtractException.Assert("ELI23850",
                    "Report contains parameters not defined in the XML file!",
                    numberOfParameters == numberOfParametersSet,
                    "Report Parameter Count", reportParameters.Count,
                    "Parameters Set Count", numberOfParametersSet);

                return true;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25075", ex);
                ee.AddDebugData("Report File Name", _reportFileName, false);
                throw ee;
            }
        }

        /// <overload>Sets all parameter fields that have the specified parameter name
        /// on the report (and any sub-reports).</overload>
        /// <summary>
        /// Sets the specified Crystal reports parameter to the specified value.
        /// </summary>
        /// <param name="parameters">The collection of report parameters.</param>
        /// <param name="parameterName">The name of the parameter to set.</param>
        /// <param name="value">The value to set on the parameter.</param>
        /// <returns>The number of parameters that were set.</returns>
        /// <exception cref="ExtractException">If the specified parameter was not
        /// set on the report.</exception>
        private int SetParameterValues(ParameterFieldDefinitions parameters,
            string parameterName, object value)
        {
            return SetParameterValues(parameters, parameterName, value, true);
        }

        /// <summary>
        /// Sets the specified Crystal reports parameter to the specified value.
        /// </summary>
        /// <param name="parameters">The collection of report parameters.</param>
        /// <param name="parameterName">The name of the parameter to set.</param>
        /// <param name="value">The value to set on the parameter.</param>
        /// <param name="exceptionIfNotSet">If <see langword="true"/> and the
        /// specified parameter is not set then an exception will be thrown.
        /// If <see langword="false"/> then no exception will be thrown and the
        /// return value will be 0.</param>
        /// <returns>The number of parameters that were set.</returns>
        private int SetParameterValues(ParameterFieldDefinitions parameters,
            string parameterName, object value, bool exceptionIfNotSet)
        {
            // Loop through all the parameters on the report looking for
            // parameters with a matching name.  Count each one that is set.
            int numSet = 0;
            foreach (ParameterFieldDefinition parameter in parameters)
            {
                // Only set non-linked parameters (linked parameters are automatically
                // set at run time).
                if (parameter.ParameterFieldName == parameterName && !parameter.IsLinked())
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
        /// Sets the specified Crystal reports parameter to the specified value.
        /// </summary>
        /// <param name="parameter">The parameter to set.</param>
        /// <param name="value">The value to set on the parameter.</param>
        private static void SetParameterValue(ParameterFieldDefinition parameter, object value)
        {
            try
            {
                ParameterDiscreteValue newValue = new ParameterDiscreteValue();
                newValue.Value = value;
                ParameterValues currentValues = parameter.CurrentValues;
                currentValues.Clear();
                currentValues.Add(newValue);
                parameter.ApplyCurrentValues(currentValues);
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
        private bool MissingParameterValue()
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
        private void ParseParameterFile()
        {
            string xmlFileName = ComputeXmlFileName();

            // Check for file existence, if there is no xml file there are no
            // parameters to prompt for
            if (!File.Exists(xmlFileName))
            {
                return;
            }

            // Load the XML file into a string
            string xml = File.ReadAllText(xmlFileName, Encoding.ASCII);

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
                    ExtractException ee =  new ExtractException("ELI23726",
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
        private string ComputeXmlFileName()
        {
            string xmlFileName = _reportFileName;
            xmlFileName = Path.GetDirectoryName(xmlFileName) + Path.DirectorySeparatorChar +
                Path.GetFileNameWithoutExtension(xmlFileName) + ".xml";
            return xmlFileName;
        }

        /// <summary>
        /// Reads the parameters from the specified <see cref="XmlTextReader"/>.
        /// </summary>
        /// <param name="xmlReader">The <see cref="XmlTextReader"/> to
        /// read the parameters from.</param>
        private void ReadParameters(XmlTextReader xmlReader)
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
                    string defaultVal = xmlReader.Value;

                    try
                    {
                        IExtractReportParameter param = null;
                        switch (paramType)
                        {
                            case "TextParameter":
                                {
                                    if (string.IsNullOrEmpty(defaultVal))
                                    {
                                        param = new TextParameter(paramName);
                                    }
                                    else
                                    {
                                        param = new TextParameter(paramName, defaultVal);
                                    }
                                }
                                break;

                            case "NumberParameter":
                                {
                                    if (string.IsNullOrEmpty(defaultVal))
                                    {
                                        param = new NumberParameter(paramName);
                                    }
                                    else
                                    {
                                        double temp = Convert.ToDouble(defaultVal,
                                            CultureInfo.InvariantCulture);
                                        param = new NumberParameter(paramName, temp);
                                    }
                                }
                                break;

                            case "DateParameter":
                                {
                                    if (string.IsNullOrEmpty(defaultVal))
                                    {
                                        param = new DateParameter(paramName);
                                    }
                                    else
                                    {
                                        DateTime temp = Convert.ToDateTime(defaultVal,
                                            CultureInfo.InvariantCulture);
                                        param = new DateParameter(paramName, temp);
                                    }
                                }
                                break;

                            case "DateRangeParameter":
                                {
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
                                            "Parameter Name", paramName);
                                        DateTime min = Convert.ToDateTime(xmlReader.Value,
                                            CultureInfo.InvariantCulture);

                                        xmlReader.MoveToAttribute("Max");
                                        ExtractException.Assert("ELI23856",
                                            "Custom date range missing 'Max' attribute!",
                                            xmlReader.Name.Equals("Max", StringComparison.Ordinal),
                                            "Parameter Name", paramName);
                                        DateTime max = Convert.ToDateTime(xmlReader.Value,
                                            CultureInfo.InvariantCulture);

                                        param = new DateRangeParameter(paramName, min, max);
                                    }
                                    else
                                    {
                                        DateRangeValue value = (DateRangeValue)Enum.Parse(
                                            typeof(DateRangeValue), defaultVal, true);
                                        param = new DateRangeParameter(paramName, value);
                                    }

                                }
                                break;

                            case "ValueListParameter":
                                {
                                    string[] values = null;
                                    bool allowOtherValues = false;

                                    // Check if the value list parameter is using a value list
                                    // or a query
                                    if (xmlReader.MoveToAttribute("Query"))
                                    {
                                        string query = xmlReader.Value;
                                        values = BuildValueListFromQuery(query);
                                    }
                                    else if (xmlReader.MoveToAttribute("Values"))
                                    {
                                        // Get the list values
                                        values = xmlReader.Value.Split(new string[] { "," },
                                            StringSplitOptions.RemoveEmptyEntries);

                                        // Get the editable attribute
                                        ExtractException.Assert("ELI23858",
                                            "Value list missing 'Editable' attribute!",
                                            xmlReader.MoveToAttribute("Editable"),
                                            "Parameter Name", paramName);
                                        if (!bool.TryParse(xmlReader.Value, out allowOtherValues))
                                        {
                                            ExtractException ee = new ExtractException("ELI23729",
                                                "Editable attribute has invalid value!");
                                            ee.AddDebugData("Attribute Value", xmlReader.Value, false);
                                            throw ee;
                                        }
                                    }
                                    else
                                    {
                                        ExtractException ee = new ExtractException("ELI28200",
                                            "Value list must contain either a 'Values' or 'Query' setting.");
                                        ee.AddDebugData("Parameter Name", paramName, false);
                                        throw ee;
                                    }

                                    param = new ValueListParameter(paramName, values, defaultVal,
                                        allowOtherValues);
                                }
                                break;

                            default:
                                {
                                    ExtractException ee = new ExtractException("ELI23730",
                                        "Unrecognized parameter type in XML file!");
                                    ee.AddDebugData("Parameter Type", xmlReader.Name, false);
                                    throw ee;
                                }
                        }

                        // Add the parameter to the collection
                        _parameters.Add(paramName, param);
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
                xmlWriter.Formatting = Formatting.Indented;
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
        /// Display the parameter entry form to the user.
        /// </summary>
        /// <returns><see langword="true"/> if the user clicked OK in parameter
        /// dialog and <see langword="false"/> if the user canceled.</returns>
        private bool DisplayParameterPrompt()
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
        private static int GetNonLinkedParameterCount(ParameterFieldDefinitions parameters)
        {
            try
            {
                int countOfParameters = 0;
                foreach (ParameterFieldDefinition parameter in parameters)
                {
                    if (!parameter.IsLinked())
                    {
                        countOfParameters++;
                    }
                }

                return countOfParameters;
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
        private void Dispose(bool disposing)
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
