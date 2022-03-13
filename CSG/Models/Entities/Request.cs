using CSG.Models.Entities.Enums;
using CSG.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CSG.Models.Entities
{
    public class Request: BaseEntity
    {

        public RequestStatus RequestStatus { get; set; } = RequestStatus.Delivered;
        public RequestType1 RequestType1 { get; set; }
        public RequestType2 RequestType2 { get; set; }
        public double LocationX { get; set; }
        public double LocationY { get; set; }
        public string ApartmentDetails { get; set; }
        public string Problem { get; set; }
        public string Explanation { get; set; }
        public decimal PurchaseAmount { get; set; } // İşlem ne kadar tuttu?
        public decimal PaidAmount { get; set; } // Ödenen tutar ne kadar?
        public List<ProductRequest> ProductRequests { get; set; } = new();//ek ürünler
        public List<ApplicationUserRequest> ApplicationUserRequests { get; set; } = new();//customer+technician
    }
}
