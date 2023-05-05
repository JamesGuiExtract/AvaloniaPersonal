using Extract.ErrorsAndAlerts.ElasticDTOs;
using System;
using System.Collections.Generic;

namespace Extract.ErrorHandling;

public static class DtoMapper
{
    public static ContextInfoDto CreateContextInfoDto(ContextInfo contextInfo)
    {
        return new()
        {
            ApplicationName = contextInfo.ApplicationName,
            ApplicationVersion = contextInfo.ApplicationVersion,
            MachineName = contextInfo.MachineName,
            DatabaseServer = contextInfo.DatabaseServer,
            DatabaseName = contextInfo.DatabaseName,
            FpsContext = contextInfo.FpsContext,
            UserName = contextInfo.UserName,
            PID = contextInfo.PID,
            FileID = contextInfo.FileID,
            ActionID = contextInfo.ActionID
        };
    }

    public static EventDto CreateEventDto(Exception ex)
    {
        if (ex == null)
        {
            return new();
        }

        ExtractException ee = (ex as ExtractException) ?? ex.AsExtractException("ELI53678");

        return new()
        {
            EliCode = ee.EliCode,
            Message = ee.Message,
            Id = ee.ExceptionIdentifier.ToString(),
            Context = CreateContextInfoDto(ee.ApplicationState),
            ExceptionTime = ee.ExceptionTime,
            Data = (ee.Data as ExceptionData)?.GetKeyValueData() ?? Array.Empty<KeyValuePair<string, string>>(),
            StackTrace = ee.StackTraceValues,
            Level = ee.LoggingLevel.Name,
            Inner = CreateEventDto(ee.InnerException),
        };
    }
}