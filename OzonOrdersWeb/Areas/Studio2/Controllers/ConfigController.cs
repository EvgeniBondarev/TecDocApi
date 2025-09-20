using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SlqStudio.Application.Services.AppSettingsServices;

namespace SlqStudio.Controllers;

[Authorize(Roles = "Admin")]
[Area("Studio2")]
public class ConfigController : Controller
    {
        private readonly IAppSettingsService _appSettingsService;
        private readonly AppSettingsBuilder _appSettingsBuilder;
        private readonly string _appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        public ConfigController(IAppSettingsService appSettingsService, AppSettingsBuilder appSettingsBuilder)
        {
            _appSettingsService = appSettingsService;
            _appSettingsBuilder = appSettingsBuilder;
        }

        // GET: /Config/Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string name, string password)
        {
            var config = _appSettingsService.ReadConfig(_appSettingsPath);
            var userSection = config["ConfigUser"];

            if (userSection != null &&
                name == userSection["Name"]?.ToString() &&
                password == userSection["Password"]?.ToString())
            {
                HttpContext.Session.SetString("IsConfigAuthorized", "true");
                return RedirectToAction("EditConfig");
            }

            ViewBag.Error = "Неверное имя пользователя или пароль.";
            return View();
        }
        
        public IActionResult EditConfig()
        {
            if (HttpContext.Session.GetString("IsConfigAuthorized") != "true")
                return RedirectToAction("Login");

            var jsonObj = _appSettingsService.ReadConfig(_appSettingsPath);
            return View(jsonObj);
        }

        [HttpPost]
        public IActionResult SaveConfig(IFormCollection form)
        {
            var updatedConfig = _appSettingsBuilder.BuildAppSettings(form);
            _appSettingsService.WriteConfig(updatedConfig, _appSettingsPath);
            return RedirectToAction("EditConfig");
        }
    }
