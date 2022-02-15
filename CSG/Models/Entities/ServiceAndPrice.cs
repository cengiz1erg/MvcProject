using CSG.Models.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSG.Models.Entities
{
    public class ServiceAndPrice: BaseEntity
    {
        public RequestType1 RequestType1 { get; set; }
        public RequestType2 RequestType2 { get; set; }
        public decimal Price { get; set; }
    }
}
