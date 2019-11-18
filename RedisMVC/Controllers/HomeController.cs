using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RedisMVC.Models;

namespace RedisMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();    
        }

        [HttpGet("/{id}")]
        public IActionResult Index(string id)
        {
            IActionResult loRespuesta;
            //string id = "id1";
            try
            {
                var loContents = ObtenerHTML(id);
                if (loContents != "") 
                    loRespuesta = Content(loContents, contentType: "text/html", Encoding.UTF8);
                else 
                    loRespuesta = View();
            }
            catch(Exception ex)
            {
                loRespuesta = BadRequest(ex.Message.ToString());
            }
            return loRespuesta;
        }

        private string ObtenerHTML(string id)
        {
            string lcUrlWS = string.Format("http://redisclientproxy-dev.us-east-2.elasticbeanstalk.com/api/values?mnUniqueIdentifier=");
            string loContents = string.Empty;
            if (!string.IsNullOrEmpty(id)) 
                lcUrlWS = string.Format(lcUrlWS + id.ToString());
            else 
                throw new Exception("El parametro no puede ser nulo");
            try
            {
                using (HttpClient loClient = new HttpClient())
                {
                    loClient.BaseAddress = new Uri(lcUrlWS);
                    loClient.DefaultRequestHeaders.Clear();
                    var loRes = loClient.GetAsync(lcUrlWS).Result;
                    if (loRes.StatusCode == HttpStatusCode.OK)
                    {
                        loContents = loRes.Content.ReadAsStringAsync().Result;
                        if (loContents.Contains("\\n\\n") || loContents.Contains("\\n"))
                        {
                            loContents = loContents.Replace("\\n\\n", "").ToString();
                            loContents = loContents.Replace("\\n", "").ToString();
                        }
                    }
                    else throw new Exception(string.Format("Error: ", loRes.StatusCode.ToString()));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(loContents + ex.Message.ToString());
            }
            return loContents;
        }    


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
