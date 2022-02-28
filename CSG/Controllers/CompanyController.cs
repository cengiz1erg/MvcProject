using AutoMapper;
using CSG.Data;
using CSG.Repository;
using CSG.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CSG.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ServiceAndPriceRepo _serviceAndPriceRepo;
        private readonly GizemContext _gizemContext;
        private readonly IMapper _mapper;
        public CompanyController(ServiceAndPriceRepo serviceAndPriceRepo , GizemContext gizemContext , IMapper mapper )
        {
            _serviceAndPriceRepo=serviceAndPriceRepo;
            _gizemContext=gizemContext;
            _mapper=mapper;
        }
        public IActionResult AboutUs()
        {
            return View();
        }
        public IActionResult ChooseUs()
        {
            return View();
        }

        public IActionResult Contacts()
        {
            return View();
        }
        public IActionResult Pricing()
        {
            //var data=_gizemContext.ServicesAndPrices
            //    .ToList()
            //    .Select(p => _mapper.Map<ServicePriceViewModel>(p))
            //    .OrderBy(x => x.price)
            //    .ToList();
               
            return View();
        }


    }
}
