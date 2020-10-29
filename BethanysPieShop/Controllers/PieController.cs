using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BethanysPieShop.Filters;
using BethanysPieShop.Models;
using BethanysPieShop.Utility;
using BethanysPieShop.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BethanysPieShop.Controllers
{
 [PieNotFoundException]
    public class PieController : Controller
    {
        private readonly IPieRepository _pieRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IPieReviewRepository _pieReviewRepository;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly ILogger<PieController> _logger;
        public PieController(IPieRepository pieRepository, ICategoryRepository categoryRepository, IPieReviewRepository pieReviewRepository, HtmlEncoder htmlEncoder, ILogger<PieController> logger)
        {
            _pieRepository = pieRepository;
            _categoryRepository = categoryRepository;
            _pieReviewRepository = pieReviewRepository;
            _htmlEncoder = htmlEncoder;
            _logger = logger;
        }

        [Route("AllPies")]
        [Route("ListOfPies")]
        public ViewResult List(string category)
        {
            IEnumerable<Pie> pies;
            string currentCategory;

            if (string.IsNullOrEmpty(category))
            {
                pies = _pieRepository.AllPies.OrderBy(p => p.PieId);
                currentCategory = "All pies";
            }
            else
            {
                pies = _pieRepository.AllPies.Where(p => p.Category.CategoryName == category)
                    .OrderBy(p => p.PieId);
                currentCategory = _categoryRepository.AllCategories.FirstOrDefault(c => c.CategoryName == category)?.CategoryName;
            }

            return View(new PiesListViewModel
            {
                Pies = pies,
                CurrentCategory = currentCategory
            });
        }


        [Route("[controller]/Details/{id}")]
        public IActionResult Details(int id)
        {
            var pie = _pieRepository.GetPieById(id);
            if (pie == null)
            {
                _logger.LogDebug(LogEventIds.GetPieIdNotFound, new Exception("Pie not found"), "Pie with id {0} not found", id);
                //return NotFound();
                //Catch this error using the exception filter
                throw new PieNotFoundException();
            }

            return View(new PieDetailViewModel() { Pie = pie });
        }

        [Route("[controller]/Details/{id}")]
        [HttpPost]
        public IActionResult Details(int id, string review)
        {
            var pie = _pieRepository.GetPieById(id);
            if (pie == null)
                return NotFound();

            //_pieReviewRepository.AddPieReview(new PieReview() { Pie = pie, Review = review });

            string encodedReview = _htmlEncoder.Encode(review);
            _pieReviewRepository.AddPieReview(new PieReview() { Pie = pie, Review = encodedReview });

            return View(new PieDetailViewModel() { Pie = pie });
        }


    }
}
