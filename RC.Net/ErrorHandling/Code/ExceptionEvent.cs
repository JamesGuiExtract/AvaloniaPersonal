//using Nest;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace Extract.ErrorHandling;

public class ExceptionEvent
{
    public string EliCode { get; set; }

    public string Message { get; set; }

    public string Id { get; set; }

    public DateTime ExceptionTime { get; set; }

    public ContextInfo Context { get; set; } = new ContextInfo();

    public IList<DictionaryEntry> Data { get; set; }

    public Stack<string> StackTrace { get; set; }

    public string Level { get; set; }

    public ExceptionEvent Inner { get; set; }

    [JsonIgnore]
    public ICommand Open_Event_Window { get; set; }

    public ExceptionEvent(Exception ex)
    {
        if (ex == null)
            return;

        var ee = (ex as ExtractException) ?? ex.AsExtractException("ELI53678");
        
        EliCode = ee.EliCode;
        Message = ee.Message;
        Id = ee.ExceptionIdentifier.ToString();
        Context = ee.ApplicationState;
        ExceptionTime = ee.ExceptionTime;
        Data = (ee.Data as ExceptionData)?.GetFlattenedData()
            .Select(d => new DictionaryEntry(d.Key, d.Value.ToString()))
            .ToList() ?? new List<DictionaryEntry>();

        StackTrace = ee.StackTraceValues;
        Level = ee.LoggingLevel.Name;
        Inner = (ee.InnerException != null) ? new ExceptionEvent(ee.InnerException) : null;
        Context.FileID = ee.FileID;
        Context.ActionID = ee.ActionID;
        Context.DatabaseServer = ee.DatabaseServer;
        Context.DatabaseName = ee.DatabaseName;
    }

    [JsonConstructor]
    public ExceptionEvent(string eliCode, string message, string id,
        ContextInfo applicationState, DateTime exceptionTime, IList<DictionaryEntry> data,
        Stack<string> stackTrace, string level, ExceptionEvent inner,
        int fileId, int actionID, string databaseServer, string databaseName, string fpsContext)
    {
        EliCode = eliCode;
        Message = message;
        Id = id;
        Context = applicationState;
        ExceptionTime = exceptionTime;
        Context.FileID= fileId;
        Context.ActionID= actionID;
        Context.DatabaseServer= databaseServer;
        Context.DatabaseName= databaseName;
        Context.FpsContext= fpsContext;
        Data = data;
        StackTrace = stackTrace;
        Level = level;
        Inner = inner;
    }

    public ExceptionEvent() { }
}