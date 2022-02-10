using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSG.Models.Entities
{
    public class ProductRequest
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public Guid RequestId { get; set; }
        public Request Request { get; set; }
    }
}
