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
    public string ELICode { get; set; }

    public string Message { get; set; }

    public string Id { get; set; }

    public DateTime ExceptionTime { get; set; }

    public ApplicationStateInfo ApplicationState { get; set; }

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
        
        ELICode = ee.EliCode;
        Message = ee.Message;
        Id = ee.ExceptionIdentifier.ToString();
        ApplicationState = ee.ApplicationState;
        ExceptionTime = ee.ExceptionTime;
        Data = (ee.Data as ExceptionData)?.GetFlattenedData()
            .Select(d => new DictionaryEntry(d.Key, d.Value.ToString()))
            .ToList() ?? new List<DictionaryEntry>();

        StackTrace = ee.StackTraceValues;
        Level = ee.LoggingLevel.Name;
        Inner = (ee.InnerException != null) ? new ExceptionEvent(ee.InnerException) : null;
    }

    [JsonConstructor]
    public ExceptionEvent(string eliCode, string message, string id,
        ApplicationStateInfo applicationState, DateTime exceptionTime, IList<DictionaryEntry> data,
        Stack<string> stackTrace, string level, ExceptionEvent inner)
    {
        ELICode = eliCode;
        Message = message;
        Id = id;
        ApplicationState = applicationState;
        ExceptionTime = exceptionTime;
        Data = data;
        StackTrace = stackTrace;
        Level = level;
        Inner = inner;
    }

    public ExceptionEvent() { }
}