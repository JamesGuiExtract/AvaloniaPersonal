using Microsoft.AspNetCore.Hosting;     // IHostingEnvironment
using Microsoft.AspNetCore.Http;        // IFormFile
using Microsoft.AspNetCore.Http.Internal;        // FormFile
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;                        // FileStream
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;

using Microsoft.Extensions.Options;


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
                            log.WriteLine($"Filename: {fileName}, \t\tLength: {file.Length}, \t\tElapsed time (mS): {timer.ElapsedMilliseconds}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
            finally
            {
                log?.Close();
            }

            return View();
        }

/*
                [HttpPost]
                public async Task<IActionResult> Index(ICollection<IFormFile> files)
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(_options.SiteSpecificUrl);
                            var token = _options.JWT;

                            foreach (var file in files)
                            {
                                if (file.Length <= 0)
                                    continue;

                                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                                var fileContent = new StreamContent(file.OpenReadStream());
                                fileContent.Headers.Add("X-FileName", fileName);
                                fileContent.Headers.Add("X-ContentType", file.ContentType);

                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                                var response = await client.PostAsync(_options.WebApiPortionOfUrl, fileContent);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = ex.Message;
                    }

                    return View();
                }
*/
        /*
        // This doesn't work for public async Task<IActionResult> SubmitFile(IFormFile file), but
        // would work for public async Task<IActionResult> SubmitFile()
        // Note that it _should_ be possible to use Microsoft.aspnetcore.http.Internal.FormFile to do this,
        // just need to figure out how to use that class - string name arg in CTOR is one issue, 
        // value for ContentDispositionHeader is another.
        [HttpPost]
        public async Task<IActionResult> Index(ICollection<IFormFile> files)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_options.SiteSpecificUrl);
                    var token = _options.JWT;

                    foreach (var file in files)
                    {
                        if (file != null && file.Length > 0)
                        {
                            try
                            {
                                byte[] data;
                                using (var br = new BinaryReader(file.OpenReadStream()))
                                {
                                    data = br.ReadBytes((int)file.OpenReadStream().Length);
                                }

                                ByteArrayContent bytes = new ByteArrayContent(data);

                                MultipartFormDataContent multiContent = new MultipartFormDataContent();
                                multiContent.Add(bytes, "file", file.FileName);

                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                                var result = await client.PostAsync(_options.WebApiPortionOfUrl, multiContent);
                                return View();
                            }
                            catch (Exception ex)
                            {
                                return BadRequest(ex.Message);
                            }
                        }

                        return BadRequest("null file");
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

            return View();
        }
        */


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
