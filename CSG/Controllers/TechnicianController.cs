using Microsoft.AspNetCore.Mvc;

namespace CSG.Controllers
{
    public class TechnicianController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
