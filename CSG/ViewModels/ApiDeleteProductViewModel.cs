using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSG.ViewModels
{
    public class ApiDeleteProductViewModel
    {
        public Guid key { get; set; }
        public string keyColumn { get; set; }
        public string action { get; set; }
    }
}
