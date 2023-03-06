using Extract.ErrorHandling;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Extract.EnvironmentLog
{
    public class Program
    {
        static DataTransferObjectSerializer _serializer = new(typeof(IExtractMeasure).Assembly);

        public static void Main(string[] args)
        {
            var tasks = new List<Task>();
            var token = new CancellationTokenSource();
            var cancellationToken = token.Token;

            //Get each implementation of IExtractLog stored in configuraton
            var activeMeasures = ConfigurationManager.AppSettings.AllKeys
                .Select(key => ConfigurationManager.AppSettings[key])
                .Select(config => _serializer.Deserialize(config.ToString()).CreateDomainObject())
                .OfType<IExtractMeasure>()
                .Where(measure => measure.Enabled)
                .ToList();

            foreach(var measurer in activeMeasures)
            {
                //Generate a task for each logger to run indefinitely over a minimum given interval
                tasks.Add(Task.Factory.StartNew((token) =>
                {
                    var ct = (CancellationToken)token;
                    while (!ct.IsCancellationRequested)
                    {
                        try
                        {
                            var data = measurer.Execute();
                            new MeasureToLog().LogEnvironmentInfo(measurer, data);
                            ct.WaitHandle.WaitOne(measurer.MeasurementInterval);
                        }
                        catch (Exception ex)
                        {
                            ex.AsExtractException("ELI54040").Log();
                        }
                    }
                }, cancellationToken, cancellationToken));
            }
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch(Exception ex)
            {
                ex.AsExtractException("ELI54041").Log();
            }
        }
    }
}
