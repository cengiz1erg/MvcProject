using CSG.Data;
using CSG.Models;
using CSG.Models.Entities;
using CSG.Models.Entities.Enums;
using CSG.Models.Identity;
using CSG.Repository;
using CSG.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CSG.Areas.Admin.Controllers
{
    public class UserApiController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ServiceAndPriceRepo _serviceAndPriceRepo;
        private readonly ProductRepo _productRepo;
        private readonly GizemContext _gizemContext;

        public UserApiController(UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ProductRepo productRepo,
            ServiceAndPriceRepo serviceAndPriceRepo,
            GizemContext gizemContext
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _serviceAndPriceRepo = serviceAndPriceRepo;
            _productRepo = productRepo;
            _gizemContext = gizemContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region UserCRUD
        public IActionResult GetUsers([FromBody] DataManagerRequest dm)
        {
            //var queryc = _gizemContext.UserRoles
            //    .Include(nameof(_gizemContext.Users))
            //    .Include(nameof(_gizemContext.Roles))
            //    .Where(ur => ur.RoleId == )
            var queryb = from ur in _gizemContext.UserRoles
                         join u in _gizemContext.Users on ur.UserId equals u.Id
                         join r in _gizemContext.Roles on ur.RoleId equals r.Id
                         where
                         r.Name == nameof(RoleNames.Technician) || r.Name == nameof(RoleNames.Operator)
                         select new ApiUserViewModel
                         {
                             id = u.Id,
                             username = u.UserName,
                             name = u.Name,
                             surname = u.SurName,
                             email = u.Email,
                             rolename = r.Name
                         };
            //var result = queryb.ToList();
            //ViewBag.dataSource = listy;
            IEnumerable DataSource = queryb.ToList();
            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                DataSource = operation.PerformSearching(DataSource, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                DataSource = operation.PerformSorting(DataSource, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                DataSource = operation.PerformFiltering(DataSource, dm.Where, dm.Where[0].Operator);
            }
            int count = DataSource.Cast<ApiUserViewModel>().Count();
            if (dm.Skip != 0)
            {
                DataSource = operation.PerformSkip(DataSource, dm.Skip);         //Paging
            }
            if (dm.Take != 0)
            {
                DataSource = operation.PerformTake(DataSource, dm.Take);
            }
            //return dm.RequiresCounts ? Json(new { result = DataSource, count = count }) : Json(DataSource);
            return Json(new { result = DataSource, count = count });
        }
        //dropdown 
        public IActionResult DataBinding()
        {
            ViewBag.Data = new List<string> { "Operator", "Technician" };
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> InsertUser([FromBody] JsonResponseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            var user = new ApplicationUser()
            {
                UserName = model.value.username,
                Name = model.value.name,
                SurName = model.value.surname,
                Email = model.value.email
            };
            var result = await _userManager.CreateAsync(user, "12312");
            if (result.Succeeded)
            {
                var result2 = await _userManager.AddToRoleAsync(user, model.value.rolename);
                Console.WriteLine();
                if (result2.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                return BadRequest();
            }
            return BadRequest();
        }
        public async Task<IActionResult> UpdateUser([FromBody] JsonResponseViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.value.id);
            var roles = await _userManager.GetRolesAsync(user);
            var removeRoleResult = await _userManager.RemoveFromRoleAsync(user, roles.FirstOrDefault());
            if (removeRoleResult.Succeeded)
            {
                var roleAddResult = await _userManager.AddToRoleAsync(user, model.value.rolename);
                if (roleAddResult.Succeeded)
                {
                    user.UserName = model.value.username;
                    user.Name = model.value.name;
                    user.SurName = model.value.surname;
                    user.Email = model.value.email;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (updateResult.Succeeded)
                    {
                        return View("Index");
                    }
                    return BadRequest();
                }
                return BadRequest();
            }
            return BadRequest();
        }
        public async Task<IActionResult> DeleteUser([FromBody] ApiDeleteUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.key);
            var deleteResult = await _userManager.DeleteAsync(user);
            if (deleteResult.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            return BadRequest();

        }
        #endregion

        #region ServicesCRU
        public IActionResult GenerateServicesAndPricesOnDb()
        {
            List<RequestType1> list1 = Enum.GetValues(typeof(RequestType1)).Cast<RequestType1>().ToList();
            List<RequestType2> list2 = Enum.GetValues(typeof(RequestType2)).Cast<RequestType2>().ToList();
            foreach (var item1 in list1)
            {
                foreach (var item2 in list2)
                {
                    ServiceAndPrice serviceAndPrice = new ServiceAndPrice()
                    {
                        RequestType1 = item1,
                        RequestType2 = item2
                    };
                    _serviceAndPriceRepo.Add(serviceAndPrice);
                }
            }
            return null;
        }
        public IActionResult GetServices()
        {
            var query = from sap in _gizemContext.ServicesAndPrices
                        select new ServicePriceViewModel
                        {
                            id = sap.Id,
                            requestType1 = sap.RequestType1.ToString(),
                            requestType2 = sap.RequestType2.ToString(),
                            price = sap.Price
                        };
            var DataSource = query.ToList();
            int count = DataSource.Cast<ServicePriceViewModel>().Count();
            return Json(new { result = DataSource, count = count });
        }
        public IActionResult UpdateService([FromBody]ApiServiceJsonViewModel model)
        {
            var data = _serviceAndPriceRepo.GetById(model.value.id);
            data.Price = model.value.price;
            _serviceAndPriceRepo.Update(data);
            return RedirectToAction(nameof(Index));
            
        }
        #endregion

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
        public IActionResult InsertProduct([FromBody]ApiProductJsonViewModel model)
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
        public IActionResult UpdateProduct([FromBody]ApiProductJsonViewModel model)
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
