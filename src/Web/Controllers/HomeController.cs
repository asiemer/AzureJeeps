using System.Threading.Tasks;
using System.Web.Mvc;
using DataGenerator;
using Domain;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.Title = "Home Page";
            var results = await DocumentDbOperations.GetAllJeeps();
            ViewBag.Jeeps = results;
            return View();
        }

        public async Task<JsonResult> TextSearch(string txt)
        {
            ViewBag.SyncOrAsync = "Asynchronous";

            return new JsonResult();
        }
    }
}
