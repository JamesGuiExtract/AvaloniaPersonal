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
    public enum BenchmarkTarget
    {
        QueueFiles,
        ProcessFiles,
        StoreVOA
    }

    [EtwProfiler(performExtraBenchmarksRun: false)]
    public class TestFileProcessing
    {
        const string FileProcessingDBRegPath = @"Software\Extract Systems\ReusableComponents\COMComponents\UCLIDFileProcessing\FileProcessingDB";
        const string UseApplicationRolesKey = "UseApplicationRoles";

        const string attributeSetName = "VOA";

        const int numberOfFiles = 100;

        FAMTestDBManager<TestFileProcessing> testDbManager;
        IDisposableDatabase<TestFileProcessing> dbWrapper;

        [ParamsAllValues]
        public BenchmarkTarget Target { get; set; } = BenchmarkTarget.StoreVOA;

        void SetRegKey(string value)
        {
            var key = Registry.LocalMachine.OpenSubKey(FileProcessingDBRegPath, true);
            key ??= Registry.LocalMachine.CreateSubKey(FileProcessingDBRegPath);
            key.SetValue(UseApplicationRolesKey, value);
        }

        void RunTest()
        {
            if (Target == BenchmarkTarget.QueueFiles)
            {
                RunQueueFilesTest();
            }
            else if (Target == BenchmarkTarget.ProcessFiles)
            {
                RunProcessFilesTest();
            }
            else if (Target == BenchmarkTarget.StoreVOA)
            {
                RunStoreVOATest();
            }
        }

        private void RunStoreVOATest()
        {
            // Add some duplicates
            foreach (int i in Enumerable.Range(1, numberOfFiles * 2))
            {
                dbWrapper.AddFakeVOA((i - 1) % numberOfFiles + 1, attributeSetName);
            }
        }

        private void RunQueueFilesTest()
        {
            foreach (int i in Enumerable.Range(1, numberOfFiles))
            {
                dbWrapper.addFakeFile(i, false);
            }
        }

        private void RunProcessFilesTest()
        {
            // Most of these calls won't return any files
            const int numberOfFilesToRetrieve = 5;
            foreach (int _ in Enumerable.Range(1, numberOfFiles))
            {
                dbWrapper.FileProcessingDB.GetFilesToProcess(dbWrapper.Actions[0], numberOfFilesToRetrieve, false, "");
            }
        }

        void CommonSetup()
        {
            GeneralMethods.TestSetup();
            testDbManager = new();
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

            if (Target == BenchmarkTarget.StoreVOA)
            {
                dbWrapper.CreateAttributeSet(attributeSetName);
            }

            if (Target != BenchmarkTarget.QueueFiles)
            {
                foreach (int i in Enumerable.Range(1, numberOfFiles))
                {
                    dbWrapper.addFakeFile(i, false);
                }
            }
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

        public static void Debug()
        {
            var test = new TestFileProcessing();
            test.GlobalSetupUseAppRole();
            //test.GlobalSetupNoAppRole();
            test.IterationSetup();
            test.UseAppRole();
            test.IterationCleanup();
            test.GlobalCleanup();
        }

        public static void RunBenchmark()
        {
            var config =
                ManualConfig
                  .Create(DefaultConfig.Instance)
                  .AddJob(
                    Job.Default
                      .WithPlatform(Platform.X86)
                      .WithRuntime(ClrRuntime.Net48)
                      .WithStrategy(RunStrategy.Monitoring)) // Monitoring uses less iterations than Throughput
                      //.WithStrategy(RunStrategy.Throughput)) // Throughput uses more iterations than Monitoring
                  .WithOptions(ConfigOptions.DisableOptimizationsValidator)
                  .AddValidator(JitOptimizationsValidator.DontFailOnError)
                  .AddLogger(ConsoleLogger.Default)
                  .AddColumnProvider(DefaultColumnProviders.Instance);

            BenchmarkRunner.Run<TestFileProcessing>(config);
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                TestFileProcessing.Debug();
            }
            else
            {
                TestFileProcessing.RunBenchmark();
            }
        }
    }
}
