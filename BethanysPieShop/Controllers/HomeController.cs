using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BethanysPieShop.Filters;
using BethanysPieShop.Models;
using BethanysPieShop.Utility;
using BethanysPieShop.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventSource;

namespace BethanysPieShop.Controllers
{

    [Route("")]
  //  [RequireHeader]
 // [ServiceFilter(typeof(TimerAction))]//This is used when you require dependency injection in your filter , and you need to register it in startup
    public class HomeController : Controller
    {

        private readonly IPieRepository _pieRepository;
        private readonly IStringLocalizer<HomeController> _stringLocalizer;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IPieRepository pieRepository, IStringLocalizer<HomeController> stringLocalizer, ILogger<HomeController> logger)
        {
            _pieRepository = pieRepository;
            _stringLocalizer = stringLocalizer;
            _logger = logger;
        }
        [Route("")]
        public IActionResult Index()
        {

            //Serilog
            _logger.LogDebug("Loading home page");

            // _logger.LogInformation(LogEventIds.LoadHomepage,"Loading Home page");
            //      throw new Exception("test");
            ViewData["PiesOfTheWeek"] = _stringLocalizer["PiesOfTheWeek"];
            ViewData["NonExistingKey"] = _stringLocalizer["NonExistingKey"];

            var homeViewModel = new HomeViewModel
            {
                PiesOfTheWeek = _pieRepository.PiesOfTheWeek
            };

            return View(homeViewModel);
        }


        [Route("TestUrl")]
        public IActionResult TestUrl()
        {
            var url =
                Url.Action("Details", "Pie", new { id = 1 });
            return RedirectToAction(url);
        }


        [Route("[controller]/SetLanguage")]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            _logger.LogInformation(LogEventIds.ChangeLanguage, "Language changed to {0}",culture);
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
}
