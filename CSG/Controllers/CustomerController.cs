using CSG.Data;
using CSG.Extensions;
using CSG.Models.Entities;
using CSG.Models.Identity;
using CSG.Repository;
using CSG.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CSG.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        //private GizemContext _gizemContext;
        private readonly RequestRepo _requestRepo;
        public CustomerController(UserManager<ApplicationUser> userManager,
                                  RequestRepo requestRepo)
        {
            _userManager = userManager;
            _requestRepo = requestRepo;
        }
        [HttpGet]
        
        public IActionResult Index()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(RequestViewModel requestViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var data = requestViewModel;
            var user = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            var request = new Request()
            {
                RequestType1 = data.RequestType1,
                RequestType2 = data.RequestType2,
                LocationX = data.LocationX,
                LocationY = data.LocationY,
                ApartmentDetails = data.ApartmentDetails,
                Problem = data.Problem,
            };
            request.ApplicationUserRequests.Add(new ApplicationUserRequest()
            {
                RequestId = request.Id,
                ApplicationUserId = user.Id
            });
            _requestRepo.Add(request);            
            return View();
        }

    }
}
