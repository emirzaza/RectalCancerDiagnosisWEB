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
        [HttpGet]
        public IActionResult login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult login(User _user)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users
                    .FirstOrDefault(u => u.Username == _user.Username && u.Password == _user.Password);

                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.UserID);

                    return RedirectToAction("Mainpage", "Main");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid username or password");
                    
                }
            }
            return View(_user);
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

        public IActionResult logout() 
        {
            HttpContext.Session.Clear();

            return RedirectToAction("login");
        }

    }
}
