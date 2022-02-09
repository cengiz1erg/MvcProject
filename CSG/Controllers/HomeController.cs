using AutoMapper;
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
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;

        public HomeController(
            UserManager<ApplicationUser> userManager, 
            RoleManager<ApplicationRole> roleManager,
            IEmailSender emailSender,
            SignInManager<ApplicationUser> signInManager,
            IMapper mapper
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _signInManager=signInManager;
            _mapper=mapper;
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
        public async Task<IActionResult> Login(RegisterLoginViewModel registerLoginViewModel)
        {
            ModelState.Remove(nameof(RegisterViewModel));
            if (!ModelState.IsValid)
            {
                ViewBag.Method = "login";
                return View(nameof(Index), registerLoginViewModel);            
            }
            var data = registerLoginViewModel.LoginViewModel;
            var result = await _signInManager.PasswordSignInAsync(data.UserName, data.Password,data.RememberMe,true);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Kullannıcı adı veya şifre hatalı");
                ViewBag.Method = "login";
                return View(nameof(Index), registerLoginViewModel);
            }
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index","Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user= await _userManager.FindByIdAsync(HttpContext.GetUserId());
            var model=_mapper.Map<UserProfileViewModel>(user);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Profile(UserProfileViewModel userProfileViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(userProfileViewModel);
            }
            var user = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            user.Name = userProfileViewModel.Name;
            user.SurName = userProfileViewModel.Surname;
            if (user.Email!=userProfileViewModel.Email)
            {
                await _userManager.RemoveFromRoleAsync(user, RoleNames.Customer);
                await _userManager.AddToRoleAsync(user, RoleNames.Passive);
            }
            user.Email = userProfileViewModel.Email;
            user.EmailConfirmed = false;

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

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, ModelState.ToFullErrorString());
            }

            return View(userProfileViewModel);

        }
        [HttpGet]
        public IActionResult PasswordUpdate()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> PasswordUpdate(PasswordUpdateViewModel passwordUpdateViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(passwordUpdateViewModel);
            }
            var user= await _userManager.FindByIdAsync(HttpContext.GetUserId());
            var result= await _userManager.ChangePasswordAsync(user,passwordUpdateViewModel.OldPassword,passwordUpdateViewModel.NewPassword);

            if (result.Succeeded)
            {
                //email gönder
                TempData["Message"] = "Şifre değiştirme işleminiz başarılı";
                return View();
            }
            else
            {
                var message = string.Join("<br>", result.Errors.Select(x => x.Description));
                TempData["Message"] = message;
                return View();
            }
        }
        [AllowAnonymous]
        public IActionResult ResetPassword()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ViewBag.Message = "Girdiğiniz email sistemimizde bulunamadı";
            }
            else
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action("ConfirmResetPassword", "Home", new { userId = user.Id, code = code },
                    protocol: Request.Scheme);

                var emailMessage = new EmailMessage()
                {
                    Contacts = new string[] { user.Email },
                    Body =
                        $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.",
                    Subject = "Reset Password"
                };
                await _emailSender.SendAsync(emailMessage);
                ViewBag.Message = "Mailinize Şifre güncelleme yönergemiz gönderilmiştir";
            }

            return View();
        }
        public IActionResult ConfirmResetPassword(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                return BadRequest("Hatalı istek");
            }

            ViewBag.Code = code;
            ViewBag.UserId = userId;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı bulunamadı");
                return View();
            }
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            var result = await _userManager.ResetPasswordAsync(user, code, model.NewPassword);

            if (result.Succeeded)
            {
                //email gönder
                TempData["Message"] = "Şifre değişikliğiniz gerçekleştirilmiştir";
                return View();
            }
            else
            {
                var message = string.Join("<br>", result.Errors.Select(x => x.Description));
                TempData["Message"] = message;
                return View();
            }
        }
    }
}
