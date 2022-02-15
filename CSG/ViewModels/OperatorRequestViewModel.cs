using CSG.Models.Entities.Enums;
using CSG.Models.Identity;
using System;

namespace CSG.ViewModels
{
    public class OperatorRequestViewModel
    {
        public Guid requestid { get; set; }
        public string username { get; set; }
        public string requesttype1 { get; set; }
        public string requesttype2 { get; set; }
        public string requeststatus { get; set; }
        public string apartmentdetails { get; set; }
        public string problem { get; set; }

    }
}
