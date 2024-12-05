using Microsoft.AspNetCore.Mvc;

namespace RectalCancerDiagnosisWeb.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult login()
        {
            return View();
        }

        public IActionResult signup()
        {
            return View();
        }
    }
}
