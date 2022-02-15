using CSG.Models.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSG.ViewModels
{
    public class ServicePriceViewModel
    {
        public Guid id { get; set; }
        public string requestType1 { get; set; }
        public string requestType2 { get; set; }
        public decimal price { get; set; }
    }
}
