using Microsoft.AspNetCore.Hosting;     // IHostingEnvironment
using Microsoft.AspNetCore.Http;        // IFormFile
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;                        // FileStream
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;


namespace MvcUploadFile.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment _environment;

        public HomeController(IHostingEnvironment env)
        {
            _environment = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        // This method works as a server-side
        /*
        [HttpPost]
        public async Task<IActionResult> Index(ICollection<IFormFile> files)
        {
            try
            {
                var uploads = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        using (var fileStream = new FileStream(Path.Combine(uploads, file.FileName), FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
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

        [HttpPost]
        public async Task<IActionResult> Index(ICollection<IFormFile> files)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://david2016svrvm");
                    //client.BaseAddress = new Uri("http://localhost:58926");

                    foreach (var file in files)
                    {
                        if (file.Length <= 0)
                            continue;

                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var fileContent = new StreamContent(file.OpenReadStream());
                        fileContent.Headers.Add("X-FileName", fileName);
                        fileContent.Headers.Add("X-ContentType", file.ContentType);

                        //var response = await client.PostAsync("api/FileItem", fileContent);
                        var response = await client.PostAsync("FileApi_VS2017/api/FileItem", fileContent);
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
