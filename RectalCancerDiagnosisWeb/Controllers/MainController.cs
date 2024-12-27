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
        private readonly string FlaskApiUrl = "http://127.0.0.1:5000/predict";

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

                    var response = await client.PostAsync(FlaskApiUrl, form);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var prediction = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                    ViewBag.Prediction = prediction;
                    ViewBag.MRIName = mriName;
                    return View("Result");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            ModelState.AddModelError("ApiError", $"Error calling Flask API: {ex.Message}");
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
        



    }

