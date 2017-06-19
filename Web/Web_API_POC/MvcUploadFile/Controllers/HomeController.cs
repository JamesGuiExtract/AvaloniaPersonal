using Microsoft.AspNetCore.Hosting;     // IHostingEnvironment
using Microsoft.AspNetCore.Http;        // IFormFile
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace MvcUploadFile.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment _environment;
        private readonly Options _options;

        public HomeController(IHostingEnvironment env, IOptions<Options> options)
        {
            _environment = env;
            _options = options.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        // This works to call public async Task<IActionResult> SubmitFile(IFormFile file).
        // Note that this could also call the same method implemenetd using a MultiPartReader.
        [HttpPost]
        public async Task<IActionResult> Index(ICollection<IFormFile> files)
        {
            Log log = null;

            try
            {
                log = new Log(_options.LogPath);

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_options.SiteSpecificUrl);
                    var token = _options.JWT;

                    foreach (var file in files)
                    {
                        if (file.Length <= 0)
                            continue;

                        var timer = Stopwatch.StartNew();

                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                        using (var content = new MultipartFormDataContent())
                        {
                            content.Add(new StreamContent(file.OpenReadStream())
                            {
                                Headers =
                                {
                                    ContentLength = file.Length,
                                    ContentType = new MediaTypeHeaderValue(file.ContentType)
                                }
                            }, "File", fileName);

                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            var response = await client.PostAsync(_options.WebApiPortionOfUrl, content);

                            timer.Stop();
                            var code = response.StatusCode;
                            log.WriteLine($"Filename: {fileName}, \t\tLength: {file.Length}, \t\tElapsed time (mS): {timer.ElapsedMilliseconds}, \t\tStatus code: {code}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                log.WriteLine($"Error reported: {msg}");
            }
            finally
            {
                log?.Close();
            }

            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
