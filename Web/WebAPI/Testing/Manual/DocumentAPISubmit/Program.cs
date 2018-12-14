using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubmitDocs
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpClient client = new HttpClient();
            string url = File.ReadAllText("url.txt").Trim();
            client.BaseAddress = new Uri(url);

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            string token = File.ReadAllText("token.txt").Trim();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Parallel.ForEach(Directory.EnumerateFiles(".", "*.pdf", SearchOption.AllDirectories)
                .Select(filename => filename), (filename) =>
            {
                Thread.Sleep(0);
                AsyncContext.Run(() => PostAsync(client, filename));
            });
        }

        static async Task PostAsync(HttpClient client, string filename)
        {
            while (true)
            {
                var content = new MultipartFormDataContent();
                var array = File.ReadAllBytes(filename);
                var imageContent = new ByteArrayContent(array);
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
                    System.Diagnostics.Trace.WriteLine(output);
                    break;
                }
            }
        }
    }
}
