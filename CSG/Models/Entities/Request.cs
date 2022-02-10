using CSG.Models.Entities.Enums;
using CSG.Models.Identity;
using System;
using System.Collections.Generic;
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
        //toplam harcamalarla ilgili property. o anki maliyetlere göre bla bla

        public List<ProductRequest> ProductRequests { get; set; } //ek masraflar(kablo,cihaz)
        public List<ApplicationUserRequest> ApplicationUserRequests { get; set; } //customer+technician
    }
}
