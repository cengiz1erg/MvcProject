using CSG.Data;
using CSG.Models.Entities;
using CSG.Repository.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSG.Repository
{
    public class RequestRepo : RepositoryBase<Request, Guid>
    {
        public RequestRepo(GizemContext gizemContext) : base(gizemContext)
        {

        }
    }
}
