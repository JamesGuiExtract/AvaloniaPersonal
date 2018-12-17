using Newtonsoft.Json.Linq;
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
            string url = null;
            string token = null;
            int batchSize = 50;

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
                    else if (flag == "URL")
                    {
                        url = args[++i];
                        continue;
                    }
                    else if (flag == "TOKENFILE")
                    {
                        tokenFile = args[++i];
                        continue;
                    }
                    else if (flag == "TOKEN")
                    {
                        token = args[++i];
                        continue;
                    }
                    else if (flag == "BATCHSIZE")
                    {
                        batchSize = int.Parse(args[++i]);
                        continue;
                    }
                }
                root = arg;
            }

            HttpClient client = new HttpClient();
            url = url ?? File.ReadAllText(urlFile).Trim();
            client.BaseAddress = new Uri(url);

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            token = token ?? File.ReadAllText(tokenFile).Trim();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var t = new Stopwatch();
            t.Start();

            var files = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                .Where(f =>
                {
                    var ext = Path.GetExtension(f).ToUpperInvariant();
                    return ext == ".PDF" || ext == ".TIF";
                });

            var mapNameToID = PostAll(client, files, batchSize).GetAwaiter().GetResult();

            Console.WriteLine(t.Elapsed);
        }

        static async Task<Dictionary<string, int>> PostAll(HttpClient client, IEnumerable<string> files, int batchSize)
        {
            var map = new Dictionary<string, int>();
            var taskBatches = files
                .Select(file => PostAsync(client, Path.GetFullPath(file)))
                .Batch(batchSize);

            foreach (var tasks in taskBatches)
            {
                while (tasks.Count > 0)
                {
                    var finished = await Task.WhenAny(tasks);
                    tasks.Remove(finished);
                    if (finished.IsCompleted)
                    {
                        map.Add(finished.Result.fileName, finished.Result.id);
                    }
                }
            }

            return map;
        }

        static async Task<(string fileName, int id)> PostAsync(HttpClient client, string filename)
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
                    var obj = JObject.Parse(await result.Content.ReadAsStringAsync());
                    var id = (int)obj["id"];
                    var output = DateTime.Now.ToLongTimeString() + " " +
                        result.StatusCode.ToString() + ": " +
                        id;
                    Console.WriteLine(output);
                    Trace.WriteLine(output);
                    return (filename, id);
                }
                else
                {
                    Console.WriteLine(result);
                    Trace.WriteLine(result);
                    throw new InvalidOperationException("Exception posting file: " + result.ToString());
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
        /// <returns>The current batch as a List</returns>
        public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> collection, int batchSize)
        {
            List<T> nextBatch = new List<T>(batchSize);
            foreach(T item in collection)
            {
                nextBatch.Add(item);
                if (nextBatch.Count == batchSize)
                {
                    yield return nextBatch;
                    nextBatch = new List<T>(batchSize);
                }
            }
            if (nextBatch.Count > 0)
            {
                yield return nextBatch;
            }
        }
    }
}
