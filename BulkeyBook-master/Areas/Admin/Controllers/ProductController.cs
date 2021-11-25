using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBook.Controllers;
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
    {
        this._unitOfWork = unitOfWork;
        this._webHostEnvironment = webHostEnvironment;
    }
    public IActionResult Index()
    {
        return View();
    }

    /**
     *  Upsert / View Product
     */

    // GET
    public IActionResult Upsert(int? id)
    {

        ProductViewModel productViewModel = new()
        {
            Products = new(),
            CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            }),

            CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            })
        };

        if (id == null || id == 0)
        {
            // Create Product
            return View(productViewModel);
        }
        else
        {
            productViewModel.Products = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
            return View(productViewModel);
            // Update Product
        }
    }

    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(ProductViewModel obj, IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string filename = Guid.NewGuid().ToString();
                var upload = Path.Combine(wwwRootPath, @"images\Products");
                var extension = Path.GetExtension(file.FileName);

                if (obj.Products.ImageUrl != null)
                {
                    var oldImagePath = Path.Combine(wwwRootPath, obj.Products.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(upload, filename + extension), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                obj.Products.ImageUrl = @"\images\Products\" + filename + extension;


            }
            if (obj.Products.Id == 0)
            {
                this._unitOfWork.Product.Add(obj.Products);
            }
            else
            {
                this._unitOfWork.Product.Update(obj.Products);

            }
            this._unitOfWork.Save();
            TempData["success"] = "Product Added successfully";
            return RedirectToAction("Index");
        }
        return View(obj);
    }

    /**
     *  Delete / View Product
     */

    // GET
    public IActionResult Remove(int? id)
    {
        if (id == null || id == 0)
            return NotFound();
        var ProductFromDb = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
        if (ProductFromDb != null)
            return View(ProductFromDb);
        else
            return NotFound();
    }

    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveProduct(int? id)
    {
        var ProductFromDb = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);

        if (ProductFromDb is null)
            return NotFound();

        this._unitOfWork.Product.Remove(ProductFromDb);
        this._unitOfWork.Product.Save();
        TempData["success"] = "Product removed successfully";
        return RedirectToAction("Index");
    }


    #region API Calls   
    [HttpGet]
    public IActionResult GetAll()
    {
        var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
        return Json(
        new
        {
            data = productList
        });
    }

    [HttpDelete]
    public IActionResult DeleteProduct(int? id)
    {
        var product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);

        if (product == null)
        {
            return Json(
                new
                {
                    success = false,
                    message = "Error while deleting product"
                }
            );
        }

        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));
        if (System.IO.File.Exists(oldImagePath))
        {
            System.IO.File.Delete(oldImagePath);
        }
        this._unitOfWork.Product.Remove(product);
        this._unitOfWork.Save();
        TempData["success"] = "Product removed successfully";
        return Json(new { success = true, message = "Product deleted successfully" });
    }
    #endregion
}

