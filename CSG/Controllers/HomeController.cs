using CSG.Models;
using CSG.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CSG.Controllers
{
    public class HomeController : Controller
    {
        

        public HomeController()
        {
          
        }

        public IActionResult Index()
        {
            return View();
        }

        
        public IActionResult Register(RegisterLoginViewModel registerLoginViewModel)
        {
            ModelState.Remove(nameof(LoginViewModel));
            if (!ModelState.IsValid)
            {
                ViewBag.Method = "register";
                return View(nameof(Index), registerLoginViewModel);
            }
            var data = registerLoginViewModel.RegisterViewModel;
            
            return View(nameof(Index));
        }
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
