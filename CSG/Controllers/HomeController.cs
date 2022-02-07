using CSG.Extensions;
using CSG.Models;
using CSG.Models.Identity;
using CSG.Services;
using CSG.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace CSG.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IEmailSender _emailSender;

        public HomeController(
            UserManager<ApplicationUser> userManager, 
            RoleManager<ApplicationRole> roleManager,IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            CheckRoles();
        }

        private void CheckRoles()
        {
            foreach (var roleName in RoleNames.Roles)
            {
                if (!_roleManager.RoleExistsAsync(roleName).Result)
                {
                    var result = _roleManager.CreateAsync(new ApplicationRole()
                    {
                        Name = roleName
                    }).Result;
                }
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterLoginViewModel registerLoginViewModel)
        {
            ModelState.Remove(nameof(LoginViewModel));
            if (!ModelState.IsValid)
            {
                ViewBag.Method = "register";
                return View(nameof(Index), registerLoginViewModel);
            }
            var data = registerLoginViewModel.RegisterViewModel;

            var user = await _userManager.FindByNameAsync(data.UserName);
            if (user != null)
            {
                ModelState.AddModelError(nameof(registerLoginViewModel.RegisterViewModel.UserName), "Bu kullanıcı adı daha önce sisteme kayıt edilmiştir.");
                ViewBag.Method = "register";
                return View(nameof(Index), registerLoginViewModel);
            }
            user = await _userManager.FindByEmailAsync(data.Email);
            if (user != null)
            {
                ModelState.AddModelError(nameof(registerLoginViewModel.RegisterViewModel.Email), "Bu email daha önce sisteme kayıt edilmiştir");
                ViewBag.Method = "register";
                return View(nameof(Index), registerLoginViewModel);
            }
            user = new ApplicationUser()
            {
                UserName = data.UserName,
                Email = data.Email,
                SurName = data.Surname,
                Name = data.Name,
            };
            var result= await _userManager.CreateAsync(user, data.Password);
            if (result.Succeeded)
            {
                //kullanıcıya rol atama
                var count=_userManager.Users.Count();
                result= await _userManager.AddToRoleAsync(user,count==1 ? RoleNames.Admin:RoleNames.Passive);


                //email doğrulama
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action("ConfirmEmail", "Home", new { userId = user.Id, code = code },
                    protocol: Request.Scheme);

                var emailMessage = new EmailMessage()
                {
                    Contacts = new string[] { user.Email },
                    Body =
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.",
                    Subject = "Confirm your email"
                };

                await _emailSender.SendAsync(emailMessage);
            }
            else
            {
                ModelState.AddModelError(string.Empty, ModelState.ToFullErrorString());
                return View("Index", data);
            }

            return View("Index");
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId,string code) 
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            ViewBag.StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";

            if (result.Succeeded && _userManager.IsInRoleAsync(user, RoleNames.Passive).Result)
            {
                await _userManager.RemoveFromRoleAsync(user, RoleNames.Passive);
                await _userManager.AddToRoleAsync(user, RoleNames.Customer);
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(RegisterLoginViewModel registerLoginViewModel)
        {
            ModelState.Remove(nameof(RegisterViewModel));
            if (!ModelState.IsValid)
            {
                ViewBag.Method = "login";
                return View(nameof(Index), registerLoginViewModel);            
            }
            var data = registerLoginViewModel.LoginViewModel;

            return View(nameof(Index));
        }


    }
}
