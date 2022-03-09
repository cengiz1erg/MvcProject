using CSG.Data;
using CSG.Models.Entities;
using CSG.Models.Entities.Enums;
using CSG.Repository;
using CSG.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSG.Areas.Admin.Controllers
{
    public class ServiceApiController : Controller
    {
        private readonly GizemContext _gizemContext;
        private readonly ServiceAndPriceRepo _serviceAndPriceRepo;
        public ServiceApiController(GizemContext gizemcontext , ServiceAndPriceRepo serviceAndPriceRepo)
        {
            _gizemContext=gizemcontext;
            _serviceAndPriceRepo=serviceAndPriceRepo;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region ServicesCRU
        public IActionResult GenerateServicesAndPricesOnDb()
        {
            List<RequestType1> list1 = Enum.GetValues(typeof(RequestType1)).Cast<RequestType1>().ToList();
            List<RequestType2> list2 = Enum.GetValues(typeof(RequestType2)).Cast<RequestType2>().ToList();
            foreach (var item1 in list1)
            {
                foreach (var item2 in list2)
                {
                    ServiceAndPrice serviceAndPrice = new ServiceAndPrice()
                    {
                        RequestType1 = item1,
                        RequestType2 = item2
                    };
                    _serviceAndPriceRepo.Add(serviceAndPrice);
                }
            }
            return null;
        }
        public IActionResult GetServices()
        {
            var query = from sap in _gizemContext.ServicesAndPrices
                        select new ServicePriceViewModel
                        {
                            id = sap.Id,
                            requestType1 = sap.RequestType1.ToString(),
                            requestType2 = sap.RequestType2.ToString(),
                            price = sap.Price
                        };
            var DataSource = query.ToList();
            int count = DataSource.Cast<ServicePriceViewModel>().Count();
            return Json(new { result = DataSource, count = count });
        }
        public IActionResult UpdateService([FromBody] ApiServiceJsonViewModel model)
        {
            var data = _serviceAndPriceRepo.GetById(model.value.id);
            data.Price = model.value.price;
            _serviceAndPriceRepo.Update(data);
            return RedirectToAction(nameof(Index));

        }
        #endregion
    }
}
