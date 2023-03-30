using Extract.ErrorHandling;
using NLog;
using NLog.Config;

if (args.Length < 1)
{
    Console.WriteLine("Usage: ConvertUEXLogs <input file name>");
    Console.WriteLine("\tConvertUEXlogs will output a .txt file that has a line for each exception in the uex file given");
    return;
}


var inputfile = args[0];
string outputFile = inputfile.Replace(".uex", ".json");

var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
string configPath = Path.Combine(commonAppData, "Extract Systems\\Configuration\\NLog-ConvertUEXLogs.config");

NLog.LogManager.Configuration = new XmlLoggingConfiguration(configPath);

var config = LogManager.Configuration;

// The variable OutputFileName can be used in the config file to specify the output file to log to.
config.Variables.Add("OutputFileName", outputFile);

ContextInfo currentAppInfo = new();

var exceptionLines = File.ReadAllLines(inputfile).ToList();
foreach (var exceptionLine in exceptionLines)
{
    try
    {
        var logElements = exceptionLine.Split(',');
        Dictionary<string, object> properties = new Dictionary<string, object>();
        properties.Add("ApplicationNameAndVersion", logElements[1]);
        properties.Add("ComputerName", logElements[2]);
        properties.Add("UserName", logElements[3]);
        properties.Add("PID", Int32.Parse(logElements[4]));
        properties.Add("ExceptionTime", Int64.Parse(logElements[5]));

        var exception = logElements[6];
        
        var ex = ExtractException.LoadFromByteStream(exception);
        LogManager.Configuration = config;
        

        bool exceptionDoesNotContainsApplicationData = ex.ApplicationState.ApplicationName.Equals(currentAppInfo.ApplicationName);
        if (exceptionDoesNotContainsApplicationData)
        {
            var ExceptionDateTime = DateTimeOffset.FromUnixTimeSeconds((Int64)properties["ExceptionTime"]).ToLocalTime().DateTime;
            ex.ExceptionTime = ExceptionDateTime;

            ex.ApplicationState.ApplicationName = ((string)properties["ApplicationNameAndVersion"]).Split('-', '|').First().Trim();
            ex.ApplicationState.ApplicationVersion = ((string)properties["ApplicationNameAndVersion"]).Split('-', '|').Last().Trim();
            ex.ApplicationState.PID = (UInt32)properties["PID"];
        }

        if (ex.Message.StartsWith("Application trace", StringComparison.InvariantCultureIgnoreCase))
        {
            ex.LogTrace();
        }
        else
        {
            ex.LogError();
        }
    }
    catch (Exception) { };
}

