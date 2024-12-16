using Microsoft.AspNetCore.Mvc;

namespace RectalCancerDiagnosisWeb.Controllers
{
    public class MainController : Controller
    {
        public IActionResult Mainpage()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            ViewBag.UserId = userId;
            return View();
        }

        public IActionResult previousResults()
        {
            return View();
        }

        public IActionResult accountCenter()
        {
            return View();
        }


    }
}
