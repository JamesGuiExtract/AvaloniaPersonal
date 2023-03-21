using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using AlertManager.Services;
using AlertManager.Benchmark.Populator;
using AlertManager.Models.AllDataClasses;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Validators;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Environments;

namespace Extract.ErrorsAndAlerts.AlertManager.Benchmark
{
    /// <summary>
    /// Class containing benchmark tests for elastic search environment information queries.
    /// </summary>
    public class ElasticSearchBenchmark
    {
        private ElasticSearchService elasticClient = new();

        [Benchmark]
        public void QueryEnvironmentByContext()
        {
            _ = elasticClient.TryGetInfoWithContextType(DateTime.Now, "Machine");
        }

        [Benchmark]
        public void QueryEnvironmentByKey()
        {
            _ = elasticClient.TryGetInfoWithDataEntry(DateTime.Now, "Version");
        }

        [Benchmark]
        public void QueryUnresolvedAlerts()
        {
            _ = elasticClient.GetUnresolvedAlerts(1);
        }

        [Benchmark]
        public void QueryAlertById()
        {
            _ = elasticClient.GetAlertById("1");
        }

        [Benchmark]
        public void QueryEnvironmentsByContextAndEntity()
        {
            _ = elasticClient.GetEnvInfoWithContextAndEntity(DateTime.Now, "Machine", "Server1");
        }

        public static void RunBenchmark()
        {
            var config =
                ManualConfig
                  .Create(DefaultConfig.Instance)
                  .AddJob(
                    Job.Default
                      .WithPlatform(Platform.X86)
                      .WithToolchain(BenchmarkDotNet.Toolchains.InProcess.Emit.InProcessEmitToolchain.Instance)
                      //.WithStrategy(RunStrategy.Monitoring)) // Monitoring uses less iterations than Throughput
                      .WithStrategy(RunStrategy.Throughput)) // Throughput uses more iterations than Monitoring
                  .WithOptions(ConfigOptions.DisableOptimizationsValidator)
                  .AddValidator(JitOptimizationsValidator.DontFailOnError)
                  .AddLogger(ConsoleLogger.Default)
                  .AddColumnProvider(DefaultColumnProviders.Instance);

            BenchmarkRunner.Run<ElasticSearchBenchmark>(config);
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                ElasticSearchBenchmarkPopulator populator = new ElasticSearchBenchmarkPopulator();

                if (args.Contains("BulkIndexAlerts"))
                {
                    populator.BulkIndexAlerts();
                }

                if (args.Contains("BulkIndexEnvironments"))
                {
                    populator.BulkIndexEnvironments();
                }

                //Used for breakpoint during testing
                _ = 1;
            }
            else
            {
                ElasticSearchBenchmark.RunBenchmark();
            }
        }
    }
}
