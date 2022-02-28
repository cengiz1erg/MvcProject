using AutoMapper;
using CSG.Areas.Admin.ViewModels;
using CSG.Extensions;
using CSG.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CSG.Areas.Admin.Controllers
{
    public class ManageController :AdminBaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        public ManageController(UserManager<ApplicationUser> userManager , IMapper mapper)
        {
            _userManager=userManager;
            _mapper=mapper;
        }
        public async Task<IActionResult> Index()
        {
            var user= await _userManager.FindByIdAsync(HttpContext.GetUserId());
            return View();
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }
        
    }
}
