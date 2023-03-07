using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using AlertManager.Services;

namespace Extract.ErrorsAndAlerts.AlertManager.Benchmark
{
    /// <summary>
    /// Class containing benchmark tests for elastic search environment information queries.
    /// </summary>
    public class EnvironmentInformationBenchmark
    {
        private EnvironmentInformationElasticsearch elasticClient = new();

        [Benchmark]
        public void QueryByContext()
        {
            _ = elasticClient.TryGetInfoWithContextType(DateTime.Now, "client");
        }

        [Benchmark]
        public void QueryByKey()
        {
            _ = elasticClient.TryGetInfoWithDataEntry(DateTime.Now, "name");
        }

        public static void RunBenchmark()
        {
            BenchmarkRunner.Run<EnvironmentInformationBenchmark>();
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
                EnvironmentInformationBenchmark.RunBenchmark();
            }
        }
    }
}
