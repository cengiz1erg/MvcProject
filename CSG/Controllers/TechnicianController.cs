using CSG.Data;
using CSG.Extensions;
using CSG.Models;
using CSG.Models.Entities;
using CSG.Models.Entities.Enums;
using CSG.Models.Identity;
using CSG.Repository;
using CSG.Services;
using CSG.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
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
        private readonly IEmailSender _emailSender;

        public TechnicianController(GizemContext gizemContext, UserManager<ApplicationUser> userManager, ProductRepo productRepo, SignInManager<ApplicationUser> signInManager , RequestRepo requestRepo, IEmailSender emailSender)
        {
            _gizemContext = gizemContext;
            _userManager = userManager;
            _productRepo = productRepo;
            _signInManager = signInManager;
            _requestRepo = requestRepo;
            _emailSender = emailSender;
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

        public IActionResult AddProducts(ProductAndRequestViewModel result)
        {
            if (result.productids is not null)
            {
                
                foreach (string productid in result.productids)
                {
                    var pr = _gizemContext.ProductRequests.FirstOrDefault(pr => pr.RequestId.ToString() == result.requestid && pr.ProductId.ToString() == productid);
                    if (pr == null)
                    {
                        ProductRequest productRequest = new ProductRequest()
                        {
                            ProductId = new Guid(productid),
                            RequestId = new Guid(result.requestid),
                            Count = 1
                        };
                        _gizemContext.ProductRequests.Add(productRequest);
                    }
                    else
                    {
                        pr.Count += 1;
                    }                   
                    _gizemContext.SaveChanges();
                }
            }
            return Json(result.requestid);
        }

        //datamanager url and update(edit)
        public IActionResult GetProducts()
        {
            var requestid = TempData["currentrequestid"].ToString();
            TempData["currentrequestid"] = requestid;
            var query = from sap in _gizemContext.ProductRequests
                        join prod in _gizemContext.Products on sap.ProductId equals prod.Id
                        join req in _gizemContext.Requests on sap.RequestId equals req.Id
                        where sap.RequestId.ToString() == requestid
                        select new ProductAndCountViewModel
                        {
                            id = sap.ProductId.ToString(),
                            productname = prod.ProductName,
                            productprice = prod.ProductPrice,
                            productcount = sap.Count
                        };
            var DataSource = query.ToList();
            int count = DataSource.Cast<ProductAndCountViewModel>().Count();
            return Json(new { result = DataSource, count = count });
        }

        public IActionResult UpdateProduct([FromBody] ProductUpdateViewModel model)
        {
            var requestid = TempData["currentrequestid"].ToString();
            TempData["currentrequestid"] = requestid;
            ProductRequest productRequest = _gizemContext.ProductRequests
                .Where(pr => pr.ProductId == model.value.id && pr.RequestId.ToString() == requestid)
                .FirstOrDefault();
            if (productRequest is not null)
            {
                productRequest.Count = model.value.productcount;
                _gizemContext.ProductRequests.Update(productRequest);
                var number = _gizemContext.SaveChanges();
                return RedirectToAction(nameof(SolveDetail));
            }
            return BadRequest();
        }

            
        public IActionResult RemoveProduct([FromBody] ApiDeleteProductViewModel model)
        {
            var requestid = TempData["currentrequestid"].ToString();
            ProductRequest productRequest = _gizemContext.ProductRequests
                .Where(pr => pr.ProductId == model.key && pr.RequestId.ToString() == requestid)
                .FirstOrDefault();
            if (productRequest is not null)
            {
                _gizemContext.Remove(productRequest);
                var number = _gizemContext.SaveChanges();
                return RedirectToAction(nameof(SolveDetail));
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult SolveDetail(string id)
        {
            if(id == null)
            {
                id = TempData["currentrequestid"].ToString();
            }
            TempData["currentrequestid"] = id;
            var products = _productRepo.Get();
            var DataSource2 = products.ToList();
            ViewBag.DataSourceProducts = DataSource2;
            ViewBag.Viewrequestid = id;
            var requestid = id;
            ViewBag.Problem = _requestRepo.GetById(new Guid(id)).Problem;
            ViewBag.ApartmentDetails = _requestRepo.GetById(new Guid(id)).ApartmentDetails;
            ViewBag.RequestType1 = _requestRepo.GetById(new Guid(id)).RequestType1.ToString();
            ViewBag.RequestType2 = _requestRepo.GetById(new Guid(id)).RequestType2.ToString();
            //ViewBag.Explanation = _requestRepo.GetById(new Guid(id)).Explanation;
            var query= _requestRepo.Get(x=>x.Id.ToString() == requestid);
            var locx = query.Select(x => x.LocationX).First().ToString();
            var locy = query.Select(x => x.LocationY).First().ToString();
            ViewBag.GetLocationX = locx;
            ViewBag.GetLocationY = locy;
            TechnicianPostViewModel technicianPostViewModel = new TechnicianPostViewModel()
            {
                Explanation = _requestRepo.GetById(new Guid(id)).Explanation,
                Id = id
            };
            return View(technicianPostViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SolveDetail(TechnicianPostViewModel model)
        {
            
            if (ModelState.IsValid)
            {                
                Request currentRequest = _requestRepo.GetById(new Guid(model.Id));

                // toplam tutar
                Decimal currentServicePrice = _gizemContext.ServicesAndPrices
                    .Where(sp => sp.RequestType1 == currentRequest.RequestType1 && sp.RequestType2 == currentRequest.RequestType2)
                    .FirstOrDefault().Price;
                Double currentProducts = _gizemContext.Requests
                    .Include(r => r.ProductRequests)
                    .ThenInclude(pr => pr.Product)
                    .Where(r => r.Id == new Guid(model.Id)) 
                    .Select(r => r.ProductRequests)
                    .AsEnumerable()
                    .Select(lst => lst.Select(pr => pr.Count * pr.Product.ProductPrice))
                    .ToList()[0]
                    .Sum();
                
                currentRequest.Explanation = model.Explanation; //request explanation from technician
                currentRequest.PurchaseAmount = (decimal)currentProducts + currentServicePrice; //Ürün tutarı
                currentRequest.RequestStatus = RequestStatus.Solved; //Solved
                _gizemContext.Requests.Update(currentRequest);
                _gizemContext.SaveChanges();

                var query = (from aur in _gizemContext.ApplicationUserRequests
                             join u in _gizemContext.Users on aur.ApplicationUserId equals u.Id
                             join r in _gizemContext.Requests on aur.RequestId equals r.Id
                             join ur in _gizemContext.UserRoles on u.Id equals ur.UserId
                             join rol in _gizemContext.Roles on ur.RoleId equals rol.Id
                             where rol.Name == RoleNames.Customer || rol.Name == RoleNames.Passive
                             where r.Id.ToString() == model.Id
                             select new
                             {
                                 userId = u.Id
                             })
                             .FirstOrDefault();

                var userId = query.userId;
                if (userId != null)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    var callbackUrl = Url.Action("Index", "Payment", new { reqId = model.Id , userId = userId }, protocol: Request.Scheme);
                    var emailMessage = new EmailMessage()
                    {
                        Contacts = new string[] { user.Email },
                        Body = $"Talebiniz çözülmüştür. <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Ödeme sayfasına gitmek için tıklayın.</a>.",
                        Subject = "Sorununuz uzman ekibimizce çözülmüştür"
                    };
                    await _emailSender.SendAsync(emailMessage);
                    // e-mail gönderildi sayfasına yönlendir
                    return RedirectToAction(nameof(Success));
                }
                else
                {
                    return RedirectToAction(nameof(Failure));
                }                 
            }
            //model hatalı teknisyenin ana sayfasına yönlendir
            return RedirectToAction("Index", new { userId = HttpContext.GetUserId() });  
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Failure()
        {
            return View();
        }

        public IActionResult UpdateExplanation(string textarea)
        {
            var requestid = TempData["currentrequestid"].ToString();
            if (textarea != null)
            {            
                Request currentRequest = _requestRepo.GetById(new Guid(requestid));
                currentRequest.Explanation = textarea;
                _gizemContext.Requests.Update(currentRequest);
                _gizemContext.SaveChanges();
                TempData["currentrequestid"] = requestid;
                return Ok();
            }
            TempData["currentrequestid"] = requestid;
            return BadRequest();
            
        }

        public IActionResult RequestId(string selectedrowreq)
        {
            //return Json(new { selectedrowreq = selectedrowreq });
            return Ok();
        }
    }
}
