using CSG.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CSG.Controllers
{
   // [Authorize]
    public class CustomerController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {

            return View();
        }
      
      
    }
}
