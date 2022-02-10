using System.Collections.Generic;

namespace CSG.Models.Entities
{
    public class Product: BaseEntity
    {
        public string ProductName { get; set; }
        public double ProductPrice { get; set; }

        public List<ProductRequest> ProductRequests { get; set; }
    }
}