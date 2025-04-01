using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ChatFlow.MVC.Models;
using ChatFlow.Domain.ViewModels;
using Newtonsoft.Json;
using ChatFlow.Domain.DTOs;
using System.Net.Http.Headers;
using System.Text;

namespace ChatFlow.MVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, HttpClient httpClient, IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;

        var baseApiUrl = _configuration.GetValue<string>("ApiBaseUrl");
        _httpClient.BaseAddress = new Uri($"{baseApiUrl}/api/");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDTO loginDTO)
    {
        var jsonContent = new StringContent(JsonConvert.SerializeObject(loginDTO), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync("auth/login", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(result);

            // Store the token in a cookie or session
            if (data.success == true)
            {
                HttpContext.Response.Cookies.Append("accessToken", data.accessToken.ToString(), new CookieOptions()
                {
                    HttpOnly = true,
                    Secure = true
                });

                return RedirectToAction("Index");
            }
        }

        ViewBag.Error = "Invalid username or password.";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        AppUserVM user = new();

        // Get the access token from cookies
        string? accessToken = HttpContext.Request.Cookies["accessToken"];

        if (!string.IsNullOrEmpty(accessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _httpClient.GetAsync("auth/getuserdatas");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                user = JsonConvert.DeserializeObject<AppUserVM>(result)!;
            }
            else
                Console.WriteLine("API Request Failed: " + response.ReasonPhrase);
        }

        return View(user);
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
