using Extract;
using Extract.AttributeFinder;
using Extract.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LearningMachineTrainer
{
    class LearningMachineTrainerApp
    {
        static bool silent;
        static bool saveErrors;
        static string uexName;

        static int Main(string[] args)
        {
            try
            {
                int usage(bool error = false)
                {
                    string message = "Usage:" +
                        "\r\n  To train/test a machine:\r\n    LearningMachineTrainer <modelFile> /csvName <name> [/testOnly] [/s] [/ef <exceptionFile>] [/cancelTokenName <name>]" +
                        "\r\n    /csvName <name> path to training or testing CSV" +
                        "\r\n    /testOnly if only testing is to be performed" +
                        "\r\n    /s = silent = no exceptions displayed" +
                        "\r\n    /ef <exceptionFile> log exceptions to file" +
                        "\r\n       (supports propagate errors to FAM option)" +
                        "\r\n       (/ef implies /s)" +
                        "\r\n    /cancelTokenName <name> used when called from another Extract application to" +
                        "\r\n       allow calling application to cancel using the named CancellationToken." +
                        "\r\n       (/cancelTokenName implies /s)";
                    MessageBox.Show(message, "LearningMachineTrainer", MessageBoxButtons.OK,
                        error ? MessageBoxIcon.Error : MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1, 0);

                    return error ? -1 : 0;
                }

                if (args.Length > 0)
                {
                    string modelFile = null;
                    string csvName = null;
                    bool testOnly = false;
                    string cancelTokenName = null;

                    for (int argNum = 0; argNum < args.Length; argNum++)
                    {
                        var arg = args[argNum];

                        if (arg.StartsWith("-", StringComparison.Ordinal)
                            || arg.StartsWith("/", StringComparison.Ordinal))
                        {
                            var val = arg.Substring(1);
                            if (val.Equals("h", StringComparison.OrdinalIgnoreCase)
                                    || val.Equals("?", StringComparison.OrdinalIgnoreCase))
                            {
                                return usage(error: false);
                            }
                            else if (val.Equals("s", StringComparison.OrdinalIgnoreCase))
                            {
                                silent = true;
                            }
                            else if (val.Equals("ef", StringComparison.OrdinalIgnoreCase))
                            {
                                saveErrors = true;
                                // /ef implies /s
                                // (else exceptions would be displayed)
                                silent = true;

                                if (++argNum < args.Length)
                                {
                                    uexName = args[argNum];
                                    continue;
                                }
                                else
                                {
                                    return usage(error: true);
                                }
                            }
                            else if (val.Equals("testOnly", StringComparison.OrdinalIgnoreCase))
                            {
                                testOnly = true;
                            }
                            else if (val.Equals("csvName", StringComparison.OrdinalIgnoreCase))
                            {
                                if (++argNum < args.Length)
                                {
                                    csvName = args[argNum];
                                    continue;
                                }
                                else
                                {
                                    return usage(error: true);
                                }
                            }
                            else if (val.Equals("cancelTokenName", StringComparison.OrdinalIgnoreCase))
                            {
                                silent = true;

                                if (++argNum < args.Length)
                                {
                                    cancelTokenName = args[argNum];
                                    continue;
                                }
                                else
                                {
                                    return usage(error: true);
                                }
                            }
                            else
                            {
                                var ue = new Exception("Unrecognized option: " + val).AsExtract("ELI45694");
                                ue.Log();

                                return usage(error: true);
                            }
                        }
                        else if (modelFile == null && File.Exists(arg))
                        {
                            modelFile = arg;
                        }
                        else
                        {
                            var ue = new Exception("Unrecognized option: " + arg).AsExtract("ELI45695");
                            ue.Log();

                            return usage(error: true);
                        }
                    }

                    NamedTokenSource namedTokenSource = null;
                    CancellationToken cancelToken = CancellationToken.None;
                    if (!string.IsNullOrWhiteSpace(cancelTokenName))
                    {
                        namedTokenSource = new NamedTokenSource(cancelTokenName);
                        cancelToken = namedTokenSource.Token;
                    }

                    return Process(modelFile, csvName, testOnly, cancelToken);
                }
                else
                {
                    return usage(error: true);
                }
            }
            catch (Exception ex)
            {
                if (saveErrors && uexName != null)
                {
                    try
                    {
                        ex.Log(uexName);
                    }
                    catch { }
                }
                else if (silent)
                {
                    ex.Log();
                }
                else
                {
                    ex.Display();
                }

                return -1;
            }
        }

        static int Process(string modelFile, string csvName, bool testOnly, CancellationToken cancelToken)
        {
            int returnCode = -1;

            // Mark start of session
            StatusArgs lastStatus = new StatusArgs
            {
                TaskName = "__START_OF_SESSION__", // Name not important, keep from matching next status
                StatusMessage = "---" // In case these logs turn into YAML files, use YAML BOF
            };

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.AutoFlush = true;
            long startOfLine = 0;
            void write(string s)
            {
                Console.Write(s);

                if (s.StartsWith("\r"))
                {
                    s = s.TrimStart('\r');
                    stream.Seek(startOfLine, SeekOrigin.Begin);
                }
                writer.Write(s);
            }
            void writeLine()
            {
                writer.WriteLine();
                Console.WriteLine();
                startOfLine = stream.Position;
            }
            void writeToLog(IEnumerable<StatusArgs> statuses)
            {
                if (!statuses.Any())
                {
                    return;
                }
                var pendingWrite = new StringBuilder();
                foreach (var status in statuses)
                {
                    // If task name and message are the same as last, combine
                    if (string.Equals(lastStatus.TaskName, status.TaskName, StringComparison.OrdinalIgnoreCase)
                        && (string.Equals(lastStatus.StatusMessage, status.StatusMessage,
                            StringComparison.OrdinalIgnoreCase)))
                    {
                        lastStatus.Combine(status);
                        write("\r");
                        write(lastStatus.GetFormattedValue());
                    }
                    // If the task name is the same and replace is specified then replace old with new
                    else if (string.Equals(lastStatus.TaskName, status.TaskName, StringComparison.OrdinalIgnoreCase)
                        && status.ReplaceLastStatus)
                    {
                        lastStatus = status;
                        write("\r");
                        write(lastStatus.GetFormattedValue());
                    }
                    // Else add last status to string pending write
                    else
                    {
                        pendingWrite.AppendLine(lastStatus.GetFormattedValue());
                        lastStatus = status;
                        writeLine();
                        write(lastStatus.GetFormattedValue());
                    }
                }
            }

            var statusUpdates = new ConcurrentQueue<StatusArgs>();

            // Update UI periodically
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var timer = new System.Timers.Timer();
            timer.Interval = 200;
            timer.Elapsed += delegate
            {
                var start = sw.ElapsedMilliseconds;
                var updates = new List<StatusArgs>();
                StatusArgs status;
                while (statusUpdates.TryDequeue(out status) && sw.ElapsedMilliseconds - start < 200)
                {
                    updates.Add(status);
                }
                writeToLog(updates);
            };
            timer.Start();

            var lm = LearningMachineModel.LoadClassifier(modelFile);

            // Write start time to log
            statusUpdates.Enqueue(new StatusArgs
                {
                    StatusMessage = new StringBuilder(4)
                      .Append("Time: ")
                      .AppendLine(DateTime.Now.ToString("s", CultureInfo.CurrentCulture))
                      .AppendLine(testOnly ? "Operation: Testing Machine" : "Operation: Training Machine")
                      .ToString()
                });

            var maintask = Task.Factory.StartNew(() =>
                testOnly
                ?  lm.TrainAndTestWithCsvData(true, null, csvName, false, a => statusUpdates.Enqueue(a), cancelToken)
                :  lm.TrainAndTestWithCsvData(false, csvName, null, false, a => statusUpdates.Enqueue(a), cancelToken)
            )
            // Handle cleanup
            .ContinueWith(task =>
            {
                Exception failed = SharedTrainingMethods.RunCleanup(task, lm, testOnly, sw, statusUpdates, cancelToken);

                // Flush log
                StatusArgs _;
                while (statusUpdates.TryPeek(out _))
                {
                    Thread.Sleep(500);
                }
                timer.Dispose();

                if (failed != null)
                {
                    if (saveErrors && uexName != null)
                    {
                        try
                        {
                            failed.Log(uexName);
                        }
                        catch { }
                    }
                    else if (silent)
                    {
                        try
                        {
                            failed.Log();
                        }
                        catch { }
                    }
                    else
                    {
                        failed.Display();
                    }
                }
                else if(!cancelToken.IsCancellationRequested)
                {
                    stream.Position = 0;
                    using (var sr = new StreamReader(stream))
                    {
                        lm.TrainingLog += ("\r\n" + sr.ReadToEnd());
                    }

                    LearningMachineModel.SaveClassifierIntoLearningMachine(lm, modelFile);

                    returnCode = 0;
                }
            }, cancelToken); // End of ContinueWith

            maintask.Wait();

            return returnCode;
        }
    }
}
