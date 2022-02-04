using CSG.Models;
using CSG.ViewModels;
using Microsoft.AspNetCore.Mvc;
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
            var data = registerLoginViewModel.RegisterViewModel;
            
            return View();
        }
        public IActionResult Login(RegisterLoginViewModel registerLoginViewModel)
        {
            var data = registerLoginViewModel.LoginViewModel;

            return View();
        }


    }
}
