using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;


namespace BulkyBook.Controllers
{
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<CoverType> objCoverTypeList = this._unitOfWork.CoverType.GetAll();
            return View(objCoverTypeList);
        }


        /**
         *  Add / View CoverType
         */

        // GET
        public IActionResult Create()
        {
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                this._unitOfWork.CoverType.Add(obj);
                this._unitOfWork.CoverType.Save();
                TempData["success"] = "Cover Type created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        /**
         *  Edit / View CoverType
         */

        // GET
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var CoverTypeFromDb = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);
            if (CoverTypeFromDb != null)
                return View(CoverTypeFromDb);
            else
                return NotFound();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                this._unitOfWork.CoverType.Update(obj);
                this._unitOfWork.CoverType.Save();
                TempData["success"] = "CoverType Edited successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        /**
         *  Delete / View CoverType
         */

        // GET
        public IActionResult Remove(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var CoverTypeFromDb = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);
            if (CoverTypeFromDb != null)
                return View(CoverTypeFromDb);
            else
                return NotFound();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveCoverType(int? id)
        {
            var CoverTypeFromDb = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);

            if (CoverTypeFromDb is null)
                return NotFound();

            this._unitOfWork.CoverType.Remove(CoverTypeFromDb);
            this._unitOfWork.CoverType.Save();
            TempData["success"] = "CoverType removed successfully";
            return RedirectToAction("Index");
        }
    }
}
