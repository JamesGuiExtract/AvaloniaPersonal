using AlertManager.Benchmark.Populator;
using AlertManager.Services;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using System.Configuration;
using Extract.ErrorsAndAlerts.ElasticDTOs;

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
            _ = elasticClient.TryGetEnvInfoWithContextType(DateTime.Now, "Machine");
        }

        [Benchmark]
        public void QueryEnvironmentByKey()
        {
            _ = elasticClient.TryGetEnvInfoWithDataEntry(DateTime.Now, "Version", "2023.3.1.42");
        }

        [Benchmark]
        public void QueryUnresolvedAlerts()
        {
            _ = elasticClient.GetUnresolvedAlerts(1);
        }

        [Benchmark]
        public void QueryAlertById()
        {
            string? testAlertId = ConfigurationManager.AppSettings["TestAlertID"];

            if (testAlertId != null)
            {
                _ = elasticClient.GetAlertById(testAlertId);
            }
        }

        [Benchmark]
        public void QueryEventsByTimeframe()
        {
            _ = elasticClient.GetEventsInTimeframe(DateTime.Now, DateTime.Now.AddDays(-5));
        }

        [Benchmark]
        public void QueryEventsByDictionaryKeyValuePair()
        {
            _ = elasticClient.GetEventsByDictionaryKeyValuePair("CatchID", "ELI123");
        }

        [Benchmark]
        public void QueryEnvironmentsByContextAndEntity()
        {
            _ = elasticClient.GetEnvInfoWithContextAndEntity(DateTime.Now, "Machine", "Server1");
        }

        public static void RunBenchmarks()
        {
            var config =
                ManualConfig
                  .Create(DefaultConfig.Instance)
                  .AddJob(
                    BenchmarkDotNet.Jobs.Job.Default
                      .WithPlatform(Platform.X86)
                      .WithToolchain(BenchmarkDotNet.Toolchains.InProcess.Emit.InProcessEmitToolchain.Instance)
                      //.WithStrategy(RunStrategy.Monitoring)) // Monitoring uses less iterations than Throughput
                      .WithStrategy(RunStrategy.Throughput)) // Throughput uses more iterations than Monitoring
                  .WithOptions(ConfigOptions.DisableOptimizationsValidator)
                  .AddValidator(JitOptimizationsValidator.DontFailOnError)
                  .AddLogger(ConsoleLogger.Default)
                  .AddColumnProvider(DefaultColumnProviders.Instance);

            ElasticSearchBenchmarkPopulator populator = new ElasticSearchBenchmarkPopulator();

            var alertIndex = ConfigurationManager.AppSettings["PopulatedAlertsTestIndex"];
            var randomAlertId = populator.GetRandomIdFromIndex<AlertDto>(alertIndex);

            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            if (settings["TestAlertID"] == null)
            {
                settings.Add("TestAlertID", randomAlertId);
            }
            else
            {
                settings["TestAlertID"].Value = randomAlertId;
            }
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);

            BenchmarkRunner.Run<ElasticSearchBenchmark>(config);
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            ElasticSearchBenchmarkPopulator populator = new ElasticSearchBenchmarkPopulator();

            if (args.Contains("BulkIndexEnvironments"))
            {
                populator.BulkIndexEnvironments();
            }

            if (args.Contains("BulkIndexEvents"))
            {
                populator.BulkIndexEvents();             
            }

            //This should come last. Populator will use existing values from environments and events indices
            if (args.Contains("BulkIndexAlerts"))
            {
                populator.BulkIndexAlerts();
            }

            //Benchmarks can't run in debug
            if (!System.Diagnostics.Debugger.IsAttached && args.Contains("RunBenchmarks"))
            {
                ElasticSearchBenchmark.RunBenchmarks();
            }

            //Used for breakpoint during testing
            _ = 1;
        }
    }
}
