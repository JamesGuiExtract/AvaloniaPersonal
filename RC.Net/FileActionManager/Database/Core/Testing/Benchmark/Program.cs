using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using Microsoft.Win32;
using System.Linq;

namespace Extract.FileActionManager.Database.Benchmark
{
    [EtwProfiler(performExtraBenchmarksRun: false)]
    public class TestFileProcessing
    {
        const string FileProcessingDBRegPath = @"Software\Extract Systems\ReusableComponents\COMComponents\UCLIDFileProcessing\FileProcessingDB";
        const string UseApplicationRolesKey = "UseApplicationRoles";

        FAMTestDBManager<TestFileProcessing> testDbManager;
        IDisposableDatabase<TestFileProcessing> dbWrapper;

        void SetRegKey(string value)
        {
            var key = Registry.LocalMachine.OpenSubKey(FileProcessingDBRegPath, true);
            key ??= Registry.LocalMachine.CreateSubKey(FileProcessingDBRegPath);
            key.SetValue(UseApplicationRolesKey, value);
        }

        void RunTest()
        {
            foreach (int i in Enumerable.Range(1, 100))
            {
                dbWrapper.addFakeFile(i, false);
            }

            dbWrapper.FileProcessingDB.GetFilesToProcess(dbWrapper.Actions[0], 10, false, "");
        }

        void CommonSetup()
        {
            GeneralMethods.TestSetup();
            testDbManager = new FAMTestDBManager<TestFileProcessing>();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            testDbManager.Dispose();
        }

        [GlobalSetup(Target = nameof(NoAppRole))]
        public void GlobalSetupNoAppRole()
        {
            SetRegKey("0");
            CommonSetup();
        }

        [GlobalSetup(Target = nameof(UseAppRole))]
        public void GlobalSetupUseAppRole()
        {
            SetRegKey("1");
            CommonSetup();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            dbWrapper = testDbManager.GetDisposableDatabase("Test_Benchmark_FileProcessing");
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            dbWrapper.Dispose();
        }

        [Benchmark(Baseline = true)]
        public void NoAppRole()
        {
            RunTest();
        }

        [Benchmark]
        public void UseAppRole()
        {
            RunTest();
        }

        static void Debug()
        {
            var test = new TestFileProcessing();
            test.GlobalSetupUseAppRole();
            test.IterationSetup();
            test.UseAppRole();
            test.IterationCleanup();
            test.GlobalCleanup();
        }

        static void RunBenchmark()
        {
            var config =
                ManualConfig
                  .Create(DefaultConfig.Instance)
                  .AddJob(
                    Job.Default
                      .WithPlatform(Platform.X86)
                      .WithRuntime(ClrRuntime.Net48)
                      .WithStrategy(RunStrategy.Monitoring)) // Monitoring uses less iterations than Throughput
                  .WithOptions(ConfigOptions.DisableOptimizationsValidator)
                  .AddValidator(JitOptimizationsValidator.DontFailOnError)
                  .AddLogger(ConsoleLogger.Default)
                  .AddColumnProvider(DefaultColumnProviders.Instance);

            BenchmarkRunner.Run<TestFileProcessing>(config);
        }

        static void Main(string[] args)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Debug();
            }
            else
            {
                RunBenchmark();
            }
        }
    }
}
