using BethanysPieShop.Utility;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BethanysPieShop.Models
{
    public class Pie
    {
        public int PieId { get; set; }
        [Remote("CheckIfPieNameAlreadyExists", "PieManagement", ErrorMessage = "That name already exists")]
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string AllergyInformation { get; set; }
        public decimal Price { get; set; }
        [ValidUrlAttribute(ErrorMessage = "That is not a valid URL")]
        public string ImageUrl { get; set; }
        [ValidUrlAttribute(ErrorMessage = "That is not a valid URL")]
        public string ImageThumbnailUrl { get; set; }
        public bool IsPieOfTheWeek { get; set; }
        public bool InStock { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int Notes { get; set; }
        public virtual List<PieReview> PieReviews { get; set; }
    }
}
