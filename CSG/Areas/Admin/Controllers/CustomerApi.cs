using Microsoft.AspNetCore.Mvc;

namespace CSG.Areas.Admin.Controllers
{
    public class CustomerApi : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
