using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SubmitDocs
{
    class Program
    {
        static void Main(string[] args)
        {
            string root = ".";
            string urlFile = "url.txt";
            string tokenFile = "token.txt";

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var flag = arg.ToUpperInvariant();
                if (flag.StartsWith("-") || flag.StartsWith("/")
                    && (i + 1) < args.Length)
                {
                    flag = flag.Substring(1);
                    if (flag == "URLFILE")
                    {
                        urlFile = args[++i];
                        continue;
                    }
                    else if (flag == "TOKENFILE")
                    {
                        tokenFile = args[++i];
                        continue;
                    }
                }
                root = arg;
            }

            HttpClient client = new HttpClient();
            string url = File.ReadAllText(urlFile).Trim();
            client.BaseAddress = new Uri(url);

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            string token = File.ReadAllText(tokenFile).Trim();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var t = new Stopwatch();
            t.Start();

            var taskBatches = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                .Where(f =>
                {
                    var ext = Path.GetExtension(f).ToUpperInvariant();
                    return ext == ".PDF" || ext == ".TIF";
                })
                .Select(file => PostAsync(client, file))
                .Batch(50);

            foreach (var tasks in taskBatches)
            {
                Task.WaitAll(tasks);
            }

            Console.WriteLine(t.Elapsed);
        }

        static async Task PostAsync(HttpClient client, string filename)
        {
            var content = new MultipartFormDataContent();
            using (var stream = File.OpenRead(filename))
            {
                var imageContent = new StreamContent(stream);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                content.Add(imageContent, "file", Path.GetFileName(filename));

                var result = await client.PostAsync("api/Document", content);

                if (result.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    var output = DateTime.Now.ToLongTimeString() + " " +
                        result.StatusCode.ToString() + ": " +
                        result.Headers.Location.AbsolutePath.Substring(
                            result.Headers.Location.AbsolutePath.LastIndexOf('/') + 1);
                    Console.WriteLine(output);
                    Trace.WriteLine(output);
                }
                else
                {
                    Console.WriteLine(result);
                    Trace.WriteLine(result);
                }
            }
        }
    }

    static class ExtensionMethods
    {
        /// <summary>
        /// Return batches of an enumerable
        /// https://www.make-awesome.com/2010/08/batch-or-partition-a-collection-with-linq/
        /// </summary>
        /// <typeparam name="T">The Type contained in the IEnumerable</typeparam>
        /// <param name="collection">The collection to return batches from</param>
        /// <param name="batchSize">The Size of each batch - last batch will just be the remaining</param>
        /// <returns>The current batch as an Array</returns>
        public static IEnumerable<T[]> Batch<T>(this IEnumerable<T> collection, int batchSize)
        {
            var nextBatch = new T[batchSize];
            var i = 0;
            foreach(T item in collection)
            {
                nextBatch[i++] = item;
                if (i == batchSize)
                {
                    yield return nextBatch;
                    nextBatch = new T[batchSize];
                    i = 0;
                }
            }
            if (i > 0)
            {
                yield return nextBatch.Where(item => item != null).ToArray();
            }
        }
    }
}
