using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using Domain;
using Newtonsoft.Json;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.Title = "Home Page";
        
            //var results = await DocumentDbOperations.GetAllJeeps();
            Listing[] results = await ReadJeeps();

            //WriteJeeps(results);

            ViewBag.Jeeps = results;
            
            return View();
        }

        public async Task<JsonResult> TextSearch(string txt)
        {
            ViewBag.SyncOrAsync = "Asynchronous";

            return new JsonResult();
        }

        public void WriteJeeps(Listing[] listings)
        {
            using (StreamWriter writer =
            new StreamWriter(Server.MapPath("~\\listings.txt")))
            {
                writer.Write(JsonConvert.SerializeObject(listings));
            }
        }

        public async Task<Listing[]> ReadJeeps()
        {
            String json = "";
            using (StreamReader sr = new StreamReader(Server.MapPath("~\\listings.txt")))
            {
                json = await sr.ReadToEndAsync();
            }

            var listings = JsonConvert.DeserializeObject<Listing[]>(json);
            return listings;
        }
    }
}
