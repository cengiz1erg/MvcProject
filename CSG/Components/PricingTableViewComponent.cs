using AutoMapper;
using CSG.Data;
using CSG.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CSG.Components
{
    public class PricingTableViewComponent:ViewComponent
    {
        private readonly GizemContext _gizemContext;
        private readonly IMapper _mapper;
        public PricingTableViewComponent(GizemContext gizemContext , IMapper mapper)
        {
            _gizemContext=gizemContext;
            _mapper=mapper;
        }
        public IViewComponentResult Invoke()
        {
            var query = from sap in _gizemContext.ServicesAndPrices
                       select new ServicePriceViewModel
                       {
                           id = sap.Id,
                           requestType1 = sap.RequestType1.ToString(),
                           requestType2 = sap.RequestType2.ToString(),
                           price = sap.Price
                       };
            var data=query.ToList()
                       .OrderBy(p => p.price)
                       .ToList();
            return View(data); 
        }
    }
}
