using CSG.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CSG.Data
{
    public class GizemContext:IdentityDbContext<ApplicationUser,ApplicationRole,string>
    {
        public GizemContext(DbContextOptions<GizemContext> options)
            : base(options)
        {
            
        }
    }
}
