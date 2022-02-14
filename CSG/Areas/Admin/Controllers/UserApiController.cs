﻿using CSG.Data;
using CSG.Models;
using CSG.Models.Identity;
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
        private GizemContext _gizemContext;
        private UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserApiController(UserManager<ApplicationUser> userManager,
            GizemContext gizemContext, RoleManager<ApplicationRole> roleManager
            )
        {
            _gizemContext = gizemContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetUsers()
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
            //var result = queryb.ToList();
            //ViewBag.dataSource = listy;
            var DataSource = queryb.ToList();
            int count = DataSource.Cast<ApiUserViewModel>().Count();
            return Json(new { result = DataSource, count = count });
        }

        public async Task<IActionResult> InsertUser([FromBody]JsonResponseViewModel model)
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

        //public IActionResult UpdateUser([FromBody] JsonResponseViewModel model)
        //{
        //    _userManager.Updat
        //    if (!ModelState.IsValid)
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return null;
        //}

        public async Task<IActionResult> TestAdd()
        {
            var user = new ApplicationUser()
            {
                UserName = "Timu",
                Name = "Timuçin",
                SurName = "Erügn",
                Email = "abc@gmail.com"
            };

            var result = await _userManager.CreateAsync(user, "12312");
            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(user, RoleNames.Technician);
            }
            return View();
        }


    }
}