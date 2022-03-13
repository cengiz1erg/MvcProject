using AutoMapper;
using CSG.Data;
using CSG.Extensions;
using CSG.Models.Entities;
using CSG.Models.Identity;
using CSG.Models.Payment;
using CSG.Repository;
using CSG.Services.Payment;
using CSG.ViewModels;
using Iyzipay.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CSG.Services.Payment;
using System.Threading;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using Geocoding;
using Geocoding.Google;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using Geocoding.MapQuest;
using Microsoft.Extensions.Configuration;
using CSG.Models.Entities.Enums;

namespace WebItProject.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly RequestRepo _requestRepo;
        private readonly GizemContext _gizemContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;


        public PaymentController(IPaymentService paymentService, RequestRepo requestRepo, GizemContext gizemContext, IMapper mapper, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _mapper = mapper;
            _paymentService = paymentService;
            _requestRepo = requestRepo;
            _gizemContext = gizemContext;
            _userManager = userManager;
            _configuration = configuration;

            var cultureInfo = CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        //[Authorize]
        public IActionResult Index(string reqId, string userId)
        {
            var currentRequest = _requestRepo.GetById(new System.Guid(reqId));
            if(currentRequest.RequestStatus == RequestStatus.Paid)
            {
                return RedirectToAction(nameof(YouPaid));
            }
            var Price = currentRequest.PurchaseAmount;
            ViewBag.Price = Price;
            TempData["reqId"] = reqId;
            TempData["userId"] = userId;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(PaymentViewModel model)
        {
            var reqId = TempData["reqId"].ToString();
            var userId = TempData["userId"].ToString();

            var currentCustomer = await _userManager.FindByIdAsync(userId);
            var currentRequest = _requestRepo.GetById(new System.Guid(reqId));

            // Sepet
            List<BasketModel> basketModels = new List<BasketModel>();
            #region Sepetİşleri
            //Sepetteki servis 
            var currentServiceAndPrice = _gizemContext.ServicesAndPrices
                .Where(sp => sp.RequestType1 == currentRequest.RequestType1 && sp.RequestType2 == currentRequest.RequestType2)
                .FirstOrDefault();
            var basketModelService = new BasketModel()
            {
                Category1 = "Service",
                ItemType = BasketItemType.PHYSICAL.ToString(),
                Id = currentServiceAndPrice.Id.ToString(), //service id
                Name = currentServiceAndPrice.RequestType1.ToString() + currentServiceAndPrice.RequestType2.ToString(),
                Price = currentServiceAndPrice.Price.ToString(new CultureInfo("en-us"))
            };
            basketModels.Add(basketModelService);

            //Sepetteki ürünler
            var Raws = (from pr in _gizemContext.ProductRequests
                                   join p in _gizemContext.Products on pr.ProductId equals p.Id
                                   join r in _gizemContext.Requests on pr.RequestId equals r.Id
                                   where r.Id.ToString() == reqId
                                   select new { pr, r, p })
                                  .ToList();
            foreach (var raw in Raws)
            {
                var basketModelProduct = new BasketModel()
                {
                    Category1 = "Product",
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Id = raw.p.Id.ToString(), //product id
                    Name = raw.p.ProductName + " - " +raw.pr.Count.ToString() + " Adet",
                    Price = (raw.p.ProductPrice * raw.pr.Count).ToString(new CultureInfo("en-us"))
                };
                basketModels.Add(basketModelProduct);
            }
            #endregion

            // Adres
            #region Adresİşleri
            double locx = _requestRepo.GetById(new System.Guid(reqId)).LocationX;
            double locy = _requestRepo.GetById(new System.Guid(reqId)).LocationY;
            // AdresModel için konumu adrese çeviren api(MapQuest) https://developer.mapquest.com
            // https://github.com/chadly/Geocoding.net
            // Install-Package Geocoding.Core
            // Install-Package Geocoding.MapQuest
            // Geocoding.MapQuest.MapQuestLocation türünde yanıt dönüyor.
            var key = _configuration.GetSection("MapQuestKey")["Key"];
            IGeocoder geocoder = new MapQuestGeocoder(key);
            IEnumerable<Geocoding.MapQuest.MapQuestLocation> addresses = (IEnumerable<MapQuestLocation>)await geocoder.ReverseGeocodeAsync(locx, locy);
            var address = addresses.ToList()[0];
            var addressModel = new AddressModel()
            {
                City = (address.City != "") ? address.City : "Unknown City",
                ContactName = $"{currentCustomer.Name} {currentCustomer.SurName}",
                Country = address.Country,
                Description = address.FormattedAddress,
                ZipCode = (address.PostCode != "") ? address.PostCode : "99999"
            };
            #endregion


            // CustomerModel
            #region Customer
            var customerModel = new CustomerModel()
            {
                City = addressModel.City,
                Country = addressModel.Country,
                Email = currentCustomer.Email,
                GsmNumber = (currentCustomer.PhoneNumber==null) ? 05448887766.ToString() : currentCustomer.PhoneNumber,//?
                Id = userId,
                IdentityNumber = userId,
                Ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                Name = currentCustomer.Name,
                Surname = currentCustomer.SurName,
                ZipCode = addressModel.ZipCode,
                LastLoginDate = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                RegistrationDate = $"{currentCustomer.CreatedDate:yyyy-MM-dd HH:mm:ss}",
                RegistrationAddress = address.FormattedAddress
            };
            #endregion

            // PaymentModel
            #region Payment
            var paymentModel = new PaymentModel()
            {
                Installment = model.Installment,
                Address = addressModel,
                BasketList = basketModels,
                Customer = customerModel,
                CardModel = model.CardModel,
                Price = currentRequest.PurchaseAmount,
                UserId = userId,
                Ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            #endregion

            InstallmentModel installmentModel = _paymentService.CheckInstallments(paymentModel.CardModel.CardNumber.Substring(0, 6), currentRequest.PurchaseAmount);

            InstallmentPriceModel ınstallmentPriceModel =
                installmentModel.InstallmentPrices.FirstOrDefault(x => x.InstallmentNumber == model.Installment);

            paymentModel.PaidPrice = decimal.Parse(ınstallmentPriceModel != null ? ınstallmentPriceModel.TotalPrice : installmentModel.InstallmentPrices[0].TotalPrice);

            PaymentResponseModel result = _paymentService.Pay(paymentModel);
            if (result.Status == "success")
            {
                currentRequest.PaidAmount = paymentModel.PaidPrice;
                currentRequest.RequestStatus = RequestStatus.Paid;
                _requestRepo.Update(currentRequest);
                return RedirectToAction(nameof(SuccessPage));

            }
            return RedirectToAction(nameof(FailurePage));
        }

        //[Authorize]
        [HttpPost]
        public IActionResult CheckInstallment(string binNumber, decimal price)
        {
            if (binNumber.Length < 6 || binNumber.Length > 16)
                return BadRequest(new
                {
                    Message = "Bad req."
                });

            var result = _paymentService.CheckInstallments(binNumber, price);
            //var result = _paymentService.CheckInstallments(binNumber, 90);
            return Ok(result);
        }

        [HttpGet]
        public IActionResult SuccessPage()
        {
            return View();
        }

        [HttpGet]
        public IActionResult FailurePage()
        {
            return View();
        }

        [HttpGet]
        public IActionResult YouPaid()
        {
            return View();
        }
    }
}