using System.ComponentModel.DataAnnotations;

namespace BulkyBook.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]

        [Display(Name = "Display Name")]
        public string Name { get; set; }

        [Range(1, 100, ErrorMessage = "Display order must be between 1 and 100 only")]
        public int DisplayOrder { get; set; }
    }
}
