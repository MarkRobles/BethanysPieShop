using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BethanysPieShop.Filters;
using BethanysPieShop.Models;
using BethanysPieShop.Utility;
using BethanysPieShop.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _cache;

        public HomeController(IPieRepository pieRepository, IStringLocalizer<HomeController> stringLocalizer,
            ILogger<HomeController> logger, IMemoryCache memoryCache, IDistributedCache cache)
        {
            _pieRepository = pieRepository;
            _stringLocalizer = stringLocalizer;
            _logger = logger;
            _memoryCache = memoryCache;
            _cache = cache;
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


            //caching change for IMemoryCache
            List<Pie> piesOfTheWeekCached = null;

            //One way of in memory caching
            //if (!_memoryCache.TryGetValue(CacheEntryConstants.PiesOfTheWeek, out piesOfTheWeekCached))
            //{
            //    piesOfTheWeekCached = _pieRepository.PiesOfTheWeek.ToList();
            //    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(30));
            //    cacheEntryOptions.RegisterPostEvictionCallback(FillCacheAgain, this);

            //    _memoryCache.Set(CacheEntryConstants.PiesOfTheWeek, piesOfTheWeekCached, cacheEntryOptions);
            //}

            //Another way of in memory caching
            piesOfTheWeekCached = _memoryCache.GetOrCreate(CacheEntryConstants.PiesOfTheWeek, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);
                entry.Priority = CacheItemPriority.High;
                return _pieRepository.PiesOfTheWeek.ToList();
            });


            //distributed cache




            var homeViewModel = new HomeViewModel
            {
                PiesOfTheWeek = piesOfTheWeekCached
            };

            return View(homeViewModel);
        }

        private void FillCacheAgain(object key, object value, EvictionReason reason, object state)
        {
            _logger.LogInformation(LogEventIds.LoadHomepage, "Cache was cleared: reason " + reason.ToString());
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
