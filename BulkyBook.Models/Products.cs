using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulkyBook.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Display Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Display Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Display ISBN")]
        public string ISBN { get; set; }

        [Required]
        [Display(Name = "Display Author")]
        public string Author { get; set; }

        [Required]
        [Display(Name = "Display List Price")]
        public double ListPrice { get; set; }

        [Required]
        [Display(Name = "Display Final Price")]
        [Range(1, 100000)]
        public double Price { get; set; }

        [Required]
        [Display(Name = "Display Special 50 Price")]
        [Range(1, 100000)]
        public double Price50 { get; set; }

        [Required]
        [Display(Name = "Display Special 100 Price")]
        [Range(1, 100000)]
        public double Price100 { get; set; }

        [ValidateNever]
        public string ImageUrl { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }

        [Required]
        [Display(Name = "Cover Type")]
        public int CoverTypeId { get; set; }
        [ForeignKey("CoverTypeId")]
        [ValidateNever]
        public CoverType CoverType { get; set; }


    }
}
