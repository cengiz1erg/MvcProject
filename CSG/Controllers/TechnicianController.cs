using CSG.Data;
using CSG.Extensions;
using CSG.Models.Identity;
using CSG.Repository;
using CSG.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CSG.Controllers
{
    public class TechnicianController : Controller
    {
        private readonly GizemContext _gizemContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ProductRepo _productRepo;
        private readonly RequestRepo _requestRepo;
        private readonly SignInManager<ApplicationUser> _signInManager; 

        public TechnicianController(GizemContext gizemContext, UserManager<ApplicationUser> userManager, ProductRepo productRepo, SignInManager<ApplicationUser> signInManager , RequestRepo requestRepo)
        {
            _gizemContext = gizemContext;
            _userManager = userManager;
            _productRepo = productRepo;
            _signInManager = signInManager;
            _requestRepo = requestRepo;
        }
        public async Task<IActionResult> Index(string userId)
        {
            string userIdd = null;
            if (userId != null)
            {
                userIdd = userId;
            }
            else
            {
                userIdd = HttpContext.GetUserId();               
            }
            var user = await _userManager.FindByIdAsync(userIdd);
            var query1 = from aur in _gizemContext.ApplicationUserRequests
                        join u in _gizemContext.Users on aur.ApplicationUserId equals u.Id
                        join r in _gizemContext.Requests on aur.RequestId equals r.Id
                        where aur.ApplicationUserId == userIdd
                         select new OperatorRequestViewModel
                        {
                            requestid = r.Id,
                            apartmentdetails = r.ApartmentDetails,
                            problem = r.Problem,
                            requesttype1 = r.RequestType1.ToString(),
                            requesttype2 = r.RequestType2.ToString(),
                            requeststatus = r.RequestStatus.ToString()
                        };
            var DataSource1 = query1.ToList();
            ViewBag.TechnicianName = $"{user.Name} {user.SurName}";
            ViewBag.DatasourceTechReq = DataSource1;

            return View();
        }
        [HttpGet]
        public IActionResult SolveDetail(string id)
        {
            var products = _productRepo.Get();
            var DataSource2 = products.ToList();
            ViewBag.DataSourceProducts = DataSource2;
            var requestid = id;
            var query= _requestRepo.Get(x=>x.Id.ToString() == requestid);
            var locx = query.Select(x => x.LocationX).First().ToString();
            var locy = query.Select(x => x.LocationY).First().ToString();
            //var ResultX = Math.Pow(10, locx.Length - 2);
            //var ResultY = Math.Pow(10, locy.Length - 2);
            //locx = String.Format("{0,0:N" + $"{locx.Length - 2}" + "}", Int64.Parse(locx) / ResultX);
            //locy = String.Format("{0,0:N" + $"{locy.Length - 2}" + "}", Int64.Parse(locy) / ResultY);
            //var decLocX= decimal.Parse(locx, new NumberFormatInfo() { NumberDecimalSeparator = "," });
            //var decLocY= decimal.Parse(locy, new NumberFormatInfo() { NumberDecimalSeparator = "," });
            ViewBag.GetLocationX = locx;
            ViewBag.GetLocationY = locy;
            return View();
        }

        [HttpPost]
        public IActionResult SolveDetail()
        {
            return View();
        }

        public IActionResult RequestId(string selectedrowreq)
        {
            //return Json(new { selectedrowreq = selectedrowreq });
            return Ok();
        }
    }
}
