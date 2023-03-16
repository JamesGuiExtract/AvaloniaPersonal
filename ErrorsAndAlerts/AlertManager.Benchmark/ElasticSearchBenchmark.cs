using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using AlertManager.Services;

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
            _ = elasticClient.TryGetInfoWithContextType(DateTime.Now, "client");
        }

        [Benchmark]
        public void QueryEnvironmentByKey()
        {
            _ = elasticClient.TryGetInfoWithDataEntry(DateTime.Now, "name");
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

        public static void RunBenchmark()
        {
            BenchmarkRunner.Run<ElasticSearchBenchmark>();
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
            }
            else
            {
                ElasticSearchBenchmark.RunBenchmark();
            }
        }
    }
}
