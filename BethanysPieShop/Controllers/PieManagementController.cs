using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BethanysPieShop.Models;
using BethanysPieShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BethanysPieShop.Controllers
{
    [Authorize(Roles = "Administrators")]
    [Authorize(Policy = "DeletePie")]
    public class PieManagementController : Controller
    {
        private readonly IPieRepository _pieRepository;
        private readonly ICategoryRepository _categoryRepository;

        public PieManagementController(IPieRepository pieRepository, ICategoryRepository categoryRepository)
        {
            _pieRepository = pieRepository;
            _categoryRepository = categoryRepository;
        }

        public ViewResult Index()
        {
            var pies = _pieRepository.AllPies.OrderBy(p => p.PieId);
            return View(pies);
        }

        public IActionResult AddPie()
        {
            var categories = _categoryRepository.AllCategories;
            var pieEditViewModel = new PieEditViewModel
            {
                Categories = categories.Select(c => new SelectListItem() { Text = c.CategoryName, Value = c.CategoryId.ToString() }).ToList(),
                CategoryId = categories.FirstOrDefault().CategoryId
            };
            return View(pieEditViewModel);
        }

        [HttpPost]
        public IActionResult AddPie(PieEditViewModel pieEditViewModel)
        {
            //Basic validation
            if (ModelState.IsValid)
            {
                _pieRepository.CreatePie(pieEditViewModel.Pie);
                return RedirectToAction("Index");
            }
            return View(pieEditViewModel);
        }
        //public IActionResult EditPie([FromRoute] int pieId)
        //public IActionResult EditPie([FromQuery]int pieId, [FromHeader] string accept)
        //public IActionResult EditPie([FromQuery]int pieId, 
        //    [FromHeader(Name = "Accept-Language")] string accept)
        public IActionResult EditPie(int pieId)
        {
            var categories = _categoryRepository.AllCategories;

            var pie = _pieRepository.AllPies.FirstOrDefault(p => p.PieId == pieId);

            var pieEditViewModel = new PieEditViewModel
            {
                Categories = categories.Select(c => new SelectListItem() { Text = c.CategoryName, Value = c.CategoryId.ToString() }).ToList(),
                Pie = pie,
                CategoryId = pie.CategoryId
            };

            var item = pieEditViewModel.Categories.FirstOrDefault(c => c.Value == pie.CategoryId.ToString());
            item.Selected = true;

            return View(pieEditViewModel);
        }

        [HttpPost]
        public IActionResult EditPie(PieEditViewModel pieEditViewModel)
        {
            pieEditViewModel.Pie.CategoryId = pieEditViewModel.CategoryId;

            if (ModelState.IsValid)
            {
                _pieRepository.UpdatePie(pieEditViewModel.Pie);
                return RedirectToAction("Index");
            }
            return View(pieEditViewModel);
        }

        [HttpPost]
        public IActionResult DeletePie(string pieId)
        {
            return View();
        }


        public IActionResult QuickEdit()
        {
            var pieNames = _pieRepository.AllPies.Select(p => p.Name).ToList();
            return View(pieNames);
        }

        [HttpPost]
        public IActionResult QuickEdit(List<string> pieNames)
        {

            


            //Do awesome things with the pie names here
            return View();
        }

        public IActionResult BulkEditPies()
        {
            var pieNames = _pieRepository.AllPies.ToList();
            return View(pieNames);
        }

        [HttpPost]
        public IActionResult BulkEditPies(List<Pie> pies)
        {   
            foreach (var item in pies)
            {
                var pie = _pieRepository.GetPieById(item.PieId);
                pie.Name = item.Name;
                _pieRepository.UpdatePie(pie);
            }
            //Do awesome things with the pie here
            return View(pies);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckIfPieNameAlreadyExists([Bind(Prefix = "Pie.Name")] string name)
        {
            var pie = _pieRepository.AllPies.FirstOrDefault(p => p.Name == name);
            return pie == null ? Json(true) : Json("That pie name is already taken");
        }

    }
}
