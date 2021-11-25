using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;


namespace BulkyBook.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<Category> objCategoryList = this._unitOfWork.Category.GetAll();
            return View(objCategoryList);
        }


        /**
         *  Add / View Category
         */

        // GET
        public IActionResult Create()
        {
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                this._unitOfWork.Category.Add(obj);
                this._unitOfWork.Category.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        /**
         *  Edit / View Category
         */

        // GET
        public IActionResult Edit(int id)
        {
            if (id == null || id == 0)
                return NotFound();
            // var categoryFromDb = _db.Categories.Find(id);
            var categoryFromDb = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);
            if (categoryFromDb != null)
                return View(categoryFromDb);
            else
                return NotFound();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                this._unitOfWork.Category.Update(obj);
                this._unitOfWork.Category.Save();
                TempData["success"] = "Category Edited successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        /**
         *  Delete / View Category
         */

        // GET
        public IActionResult Remove(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var categoryFromDb = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);
            if (categoryFromDb != null)
                return View(categoryFromDb);
            else
                return NotFound();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveCategory(int? id)
        {
            var categoryFromDb = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);

            if (categoryFromDb is null)
                return NotFound();

            this._unitOfWork.Category.Remove(categoryFromDb);
            this._unitOfWork.Category.Save();
            TempData["success"] = "Category removed successfully";
            return RedirectToAction("Index");
        }
    }
}
