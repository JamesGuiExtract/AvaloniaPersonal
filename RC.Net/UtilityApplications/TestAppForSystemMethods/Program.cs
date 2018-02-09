using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Extract.Utilities;

namespace TestAppForSystemMethods
{
    class Program
    {
        static int Main(string[] args)
        {
            // Default to 0
            int sleepTime = 0;
            bool cancelable = false;
            string cancelTokenName = "";

            for (int i = 0; i < args.Count(); i++)
            {

                if (args[i].Equals ("/SleepTime", StringComparison.InvariantCultureIgnoreCase))
                {
                    i++;
                    if (i < args.Count())
                    {
                        sleepTime = int.Parse(args[i]);
                    }
                    else
                    {
                        return -1;
                    }
                }
                else if (args[i].Equals("/CancelTokenName", StringComparison.InvariantCultureIgnoreCase))
                {
                    i++;
                    if (i < args.Count())
                    {
                        cancelable = true;
                        cancelTokenName = args[i];
                    }
                    else
                    {
                        return -1;
                    }
                }
            }

            if (cancelable)
            {
                NamedTokenSource namedTokenSource = new NamedTokenSource(cancelTokenName);
                if (namedTokenSource.Token.WaitHandle.WaitOne(sleepTime))
                {
                    if (namedTokenSource.Token.IsCancellationRequested)
                    {
                        return SystemMethods.OperationCanceledExitCode;
                    }
                }

                return SystemMethods.OperationTimeoutExitCode;
            }
            Thread.Sleep(sleepTime);

            return args.Count();
        }
    }
}
