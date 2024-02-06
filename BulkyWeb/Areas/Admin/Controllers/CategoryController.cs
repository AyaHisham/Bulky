using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Category> categoryList = _unitOfWork.Category.GetAll().ToList();
            return View(categoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category newCategory)
        {          
            if (ModelState.IsValid)
            {  
                _unitOfWork.Category.Add(newCategory);
                _unitOfWork.Save();
                TempData["success"] = "The Category Created Successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) { return NotFound(); }
            Category? selectedCategory = _unitOfWork.Category.Get(q => q.Id == id);
            if (selectedCategory == null) { return NotFound(); }
            return View(selectedCategory);
        }

        [HttpPost]
        public IActionResult Edit(Category updatedCategory)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(updatedCategory);
                _unitOfWork.Save();
                TempData["success"] = "The Category Updated Successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) { return NotFound(); }
            Category? selectedCategory = _unitOfWork.Category.Get(q => q.Id == id);
            if (selectedCategory == null) { return NotFound(); }
            return View(selectedCategory);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            if (id == null || id == 0) { return NotFound(); }
            Category? selectedCategory = _unitOfWork.Category.Get(q => q.Id == id);
            if (selectedCategory == null) { return NotFound(); }
            _unitOfWork.Category.Remove(selectedCategory);
            _unitOfWork.Save();
            TempData["success"] = "The Category Deleted Successfully";
            return RedirectToAction("Index");
        }
    }
}
