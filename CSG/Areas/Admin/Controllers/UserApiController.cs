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
      
        private readonly GizemContext _gizemContext;

        public UserApiController(UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            GizemContext gizemContext
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _gizemContext = gizemContext;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region UserCRUD
        public IActionResult GetUsers([FromBody] DataManagerRequest dm)
        {

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
            var result = await _userManager.CreateAsync(user, "P@ssword.1");
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

      

       
    }
}
