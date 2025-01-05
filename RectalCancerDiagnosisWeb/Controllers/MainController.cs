using Microsoft.AspNetCore.Mvc;
using RectalCancerDiagnosisWeb.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
public class MainController : Controller
    {
        private readonly AppDbContext _context;
    private const string FlaskApiUrl = "http://127.0.0.1:5000";


    public MainController(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            
        }

    public IActionResult Mainpage()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        ViewBag.UserId = userId;
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> UploadNii(IFormFile niiFile, string mriName)
    {
        if (niiFile == null || niiFile.Length == 0)
        {
            ModelState.AddModelError("FileError", "Please upload a valid NII file.");
            return View("Mainpage");
        }

        try
        {
            using (var client = new HttpClient())
            {
                var form = new MultipartFormDataContent();
                using (var fileStream = new MemoryStream())
                {
                    await niiFile.CopyToAsync(fileStream);
                    fileStream.Position = 0;

                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    form.Add(fileContent, "image", niiFile.FileName);

                    // Flask API URL
                    var flaskApiUrl = "http://127.0.0.1:5000/predict"; // Flask API'nizin doğru URL'si

                    var response = await client.PostAsync(flaskApiUrl, form);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    dynamic prediction = JsonConvert.DeserializeObject(jsonResponse);

                    if (prediction == null || prediction.result == null || prediction.nii_path == null)
                    {
                        ModelState.AddModelError("ApiError", "Invalid response from Flask API.");
                        return View("Mainpage");
                    }

                    // ViewBag ile tahmin sonuçlarını gönder
                    ViewBag.Prediction = prediction.result.ToString();
                    ViewBag.NiiPath = $"{flaskApiUrl.Replace("/predict", "")}/{prediction.nii_path}";
                    ViewBag.MRIName = mriName;
                    ViewBag.Visualization = prediction.visualization.ToString(); // Base64 görselleştirme

                    return View("Result");
                }
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("ApiError", $"Error communicating with Flask API: {ex.Message}");
            return View("Mainpage");
        }
    }










    public IActionResult previousResults()
        {
            List<User> userr = _context.Users.ToList();
            return View(userr);
        }
        [HttpGet]
        public IActionResult accountCenter()
        {
            return View();
        }

    [HttpPost]
        public IActionResult accountCenter(User model)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var existingUser = _context.Users.FirstOrDefault(u => u.UserID == userId);
        
        existingUser.Name = !string.IsNullOrWhiteSpace(model.Name) ? model.Name : existingUser.Name;
        existingUser.Surname = !string.IsNullOrWhiteSpace(model.Surname) ? model.Surname : existingUser.Surname;
        existingUser.Username = !string.IsNullOrWhiteSpace(model.Username) ? model.Username : existingUser.Username;
        existingUser.Password = !string.IsNullOrWhiteSpace(model.Password) ? model.Password : existingUser.Password;
        existingUser.Email = !string.IsNullOrWhiteSpace(model.Email) ? model.Email : existingUser.Email;

        _context.SaveChanges();
        return RedirectToAction("Mainpage", "Main");
    }

}






    

