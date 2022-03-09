using CSG.Data;
using CSG.Models.Entities;
using CSG.Repository;
using CSG.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace CSG.Areas.Admin.Controllers
{
    public class ProductApiController : Controller
    {
        private readonly GizemContext _gizemContext;
        private readonly ProductRepo _productRepo;

        public ProductApiController(GizemContext gizemContext , ProductRepo productRepo)
        {
            _gizemContext = gizemContext;
            _productRepo = productRepo;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region ProductCRUD
        public IActionResult GetProducts()
        {
            var query = from p in _gizemContext.Products
                        select new ProductViewModel
                        {
                            id = p.Id,
                            productname = p.ProductName,
                            productprice = p.ProductPrice.ToString()
                        };
            var DataSource = query.ToList();
            int count = DataSource.Cast<ProductViewModel>().Count();
            return Json(new { result = DataSource, count = count });
        }
        public IActionResult InsertProduct([FromBody] ApiProductJsonViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            Product product = new Product()
            {
                ProductName = model.value.productname,
                ProductPrice = double.Parse(model.value.productprice)
            };
            _productRepo.Add(product);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult UpdateProduct([FromBody] ApiProductJsonViewModel model)
        {
            var data = _productRepo.GetById(model.value.id);
            data.ProductName = model.value.productname;
            data.ProductPrice = double.Parse(model.value.productprice);
            _productRepo.Update(data);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult DeleteProduct([FromBody] ApiDeleteProductViewModel model)
        {
            Product product = _productRepo.GetById(model.key);
            _productRepo.Remove(product);
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
