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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CSG.Controllers
{
    //[Authorize]
    public class PaymentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GizemContext _gizemContext;
        private readonly ServiceAndPriceRepo _serviceAndPriceRepo;
        private readonly IPaymentService _paymentService;
        private IMapper _mapper;
        public PaymentController(UserManager<ApplicationUser> userManager ,ServiceAndPriceRepo serviceAndPriceRepo ,GizemContext gizemContext,IMapper mapper, IPaymentService paymentService)
        {
            _userManager=userManager;
            _serviceAndPriceRepo=serviceAndPriceRepo;
            _gizemContext=gizemContext;
            _paymentService=paymentService;
            _mapper=mapper;
            var cultureInfo = CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public IActionResult CheckInstallment(string binNumber, decimal price)
        {
            if (binNumber.Length < 6 || binNumber.Length > 16)
                return BadRequest(new
                {
                    Message = "Bad req."
                });

           // var result = _paymentService.CheckInstallments(binNumber, price);
            return Ok();
        }
        public IActionResult Purchase(Guid id)
        {
            var data=_serviceAndPriceRepo.GetById(id);
            if(data == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var model= _mapper.Map<ServicePriceViewModel>(data);
            ViewBag.Services = model;

            var model2 = new PaymentViewModel()
            {
                BasketModel = new BasketModel()
                {
                    Category1 = data.RequestType1.ToString(),
                    ItemType = BasketItemType.VIRTUAL.ToString(),
                    Id = data.Id.ToString(),
                    Name = data.RequestType1.ToString(),
                    Price = data.Price.ToString(new CultureInfo("en-us"))
                }
            };
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> Purchase(PaymentViewModel model)
        //{
        //    var type = await _gizemContext.ServicesAndPrices.FindAsync(Guid.Parse(model.BasketModel.Id));

        //    var basketModel = new BasketModel()
        //    {
        //        Category1 = type.RequestType2.ToString(),
        //        ItemType = BasketItemType.VIRTUAL.ToString(),
        //        Id = type.Id.ToString(),
        //        Name = type.RequestType1.ToString(),
        //        Price = type.Price.ToString(new CultureInfo("en-us"))
        //    };

        //    var user = await _userManager.FindByIdAsync(HttpContext.GetUserId());


        //    var customerModel = new CustomerModel()
        //    {
               
        //        Email = user.Email,
        //        GsmNumber = user.PhoneNumber,
        //        Id = user.Id,
        //        IdentityNumber = user.Id,
        //        Ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
        //        Name = user.Name,
        //        Surname = user.SurName,
        //        LastLoginDate = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}",
        //        RegistrationDate = $"{user.CreatedDate:yyyy-MM-dd HH:mm:ss}",
        //    };

        //    var paymentModel = new PaymentModel()
        //    {
        //        Installment = model.Installment,
        //        BasketList = new List<BasketModel>() { basketModel },
        //        Customer = customerModel,
        //        CardModel = model.CardModel,
        //        Price = model.Amount,
        //        UserId = HttpContext.GetUserId(),
        //        Ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString()
        //    };

        //    var installmentInfo = _paymentService.CheckInstallments(paymentModel.CardModel.CardNumber.Substring(0, 6), paymentModel.Price);

        //    var installmentNumber =
        //        installmentInfo.InstallmentPrices.FirstOrDefault(x => x.InstallmentNumber == model.Installment);

        //    paymentModel.PaidPrice = decimal.Parse(installmentNumber != null ? installmentNumber.TotalPrice : installmentInfo.InstallmentPrices[0].TotalPrice);

        //    //legacy code

        //    var result = _paymentService.Pay(paymentModel);
        //    return View();
        //}
    }
}
