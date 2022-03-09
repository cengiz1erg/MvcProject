using AutoMapper;
using CSG.Areas.Admin.ViewModels;
using CSG.Extensions;
using CSG.Models;
using CSG.Models.Identity;
using CSG.Services;
using CSG.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace CSG.Areas.Admin.Controllers
{
    public class ManageController :AdminBaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;
        public ManageController(UserManager<ApplicationUser> userManager ,RoleManager<ApplicationRole> roleManager, IMapper mapper , IEmailSender emailSender)
        {
            _userManager=userManager;
            _roleManager=roleManager;
            _mapper=mapper;
            _emailSender=emailSender;
        }
        public async Task<IActionResult> Index()
        {
            var user= await _userManager.FindByIdAsync(HttpContext.GetUserId());
            return View();
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
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null)
            {
                ModelState.AddModelError(nameof(model.UserName), "Bu kullanıcı adı daha önce sisteme kayıt edilmiştir.");
                ViewBag.Method = "register";
                return View(nameof(Index), model);
            }
            user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Bu email daha önce sisteme kayıt edilmiştir");
                ViewBag.Method = "register";
                return View(nameof(Index), model);
            }
            var admin = new ApplicationUser()
            {
                UserName = model.UserName,
                Email = model.Email,
                Name = model.Name,
                SurName = model.Surname,
            };
            var result=await _userManager.CreateAsync(admin,model.Password);
            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(admin,RoleNames.Admin);

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
                return View("Index", model);
            }

            return View("Index");
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, true);
            if (result.Succeeded)
            {
                var url=Url.Action("Index", "Manage" , new { area = "Admin" });
                return LocalRedirect(url);
            }
            else
            {

                ModelState.AddModelError(string.Empty, "Kullannıcı adı veya şifre hatalı");
                ViewBag.Method = "login";
                return View(nameof(Index), model);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
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
                await _userManager.AddToRoleAsync(user, RoleNames.Admin);
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
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


    }
}
