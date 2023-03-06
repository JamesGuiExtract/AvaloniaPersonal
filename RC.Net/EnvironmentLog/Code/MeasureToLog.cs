using Extract.ErrorHandling;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Extract.EnvironmentLog
{
    internal class MeasureToLog
    {
        public MeasureToLog() { }

        public void LogEnvironmentInfo(IExtractMeasure measurer, List<Dictionary<string, string>> data)
        {
            foreach (Dictionary<string, string> entry in data)
            {
                try
                {
                    EnvironmentLog log = new EnvironmentLog(
                        measurer.Customer,
                        DateTime.Now,
                        measurer.Context,
                        measurer.Entity,
                        measurer.MeasurementType,
                        entry);
                    log.Log();
                } catch (Exception ex)
                {
                    ex.AsExtractException("ELI54042").Log();
                }
            }
            LogManager.Flush();
#if DEBUG
            Console.WriteLine("doc logged, thread: " + Thread.CurrentThread.ManagedThreadId.ToString());
#endif
        }
    }
}
