using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CSG.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminBaseController : Controller
    {
    }
}
