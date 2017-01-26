using Microsoft.AspNetCore.Hosting;     // IHostingEnvironment
using Microsoft.AspNetCore.Http;        // IFormFile
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;                        // FileStream
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

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

        [HttpPost]
        public async Task<IActionResult> Index(ICollection<IFormFile> files)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    //client.BaseAddress = new Uri("http://david2016svrvm");
                    //client.BaseAddress = new Uri("http://localhost:58926");
                    client.BaseAddress = new Uri(_options.SiteSpecificUrl);

                    foreach (var file in files)
                    {
                        if (file.Length <= 0)
                            continue;

                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var fileContent = new StreamContent(file.OpenReadStream());
                        fileContent.Headers.Add("X-FileName", fileName);
                        fileContent.Headers.Add("X-ContentType", file.ContentType);

                        //var response = await client.PostAsync("api/FileItem", fileContent);
                        //var response = await client.PostAsync("FileApi_VS2017/api/FileItem", fileContent);
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
