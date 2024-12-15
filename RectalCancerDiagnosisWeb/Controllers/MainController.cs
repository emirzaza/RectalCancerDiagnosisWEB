using Microsoft.AspNetCore.Mvc;

namespace RectalCancerDiagnosisWeb.Controllers
{
    public class MainController : Controller
    {
        public IActionResult Mainpage()
        {
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
