using Microsoft.AspNetCore.Mvc;
using RectalCancerDiagnosisWeb.Models;

namespace RectalCancerDiagnosisWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public IActionResult login()
        {
            return View();
        }
        [HttpGet]
        public IActionResult signup()
        {
            return View();
        }
        [HttpPost]
        public IActionResult signup(User newuser)
        {
           _context.Users.Add(newuser);
           _context.SaveChanges();

            return RedirectToAction("login");
        }
    }
}
