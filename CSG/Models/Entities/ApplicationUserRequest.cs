using CSG.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSG.Models.Entities
{
    public class ApplicationUserRequest
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public Guid RequestId { get; set; }
        public Request Request { get; set; }
    }
}
