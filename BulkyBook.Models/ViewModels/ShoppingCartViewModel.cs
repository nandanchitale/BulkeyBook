using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBook.Models.ViewModels
{
    public class ShoppingCartViewModel
    {

        public IEnumerable<ShoppingCart> CartListItems { get; set; }

        public OrderHeader OrderHeader { get; set; }

    }
}
