using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> allProduct = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
            return View(allProduct);
        }
        // single page for update and insert based on there is id or not (update - insert)
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(q => new SelectListItem
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                }),
                Product = new Product()
            };

            if (id == null || id == 0)
            {
                //insert situation
                return View(productVM);
            }          
            else 
            {
                //update situation
                productVM.Product = _unitOfWork.Product.Get(q => q.Id == id);
                return View(productVM);
            }          
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM,IFormFile? file )
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString()+Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");
                    if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        //remove old image.
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream  = new FileStream(Path.Combine(productPath,fileName),FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }
                if (productVM.Product.Id == 0){ _unitOfWork.Product.Add(productVM.Product);}
                else { _unitOfWork.Product.Update(productVM.Product);}
                _unitOfWork.Save();
                TempData["success"] = "The Product Created Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(q => new SelectListItem
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                });
                return View(productVM);
            }
        }
      
        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> allProduct = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new {data = allProduct });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(p=>p.Id == id);
            if(productToBeDeleted == null)
            { return Json(new { success = false, message = "Error while deleting" }); }
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Product Deleted Successfully" });
        }
        #endregion
    }
}
