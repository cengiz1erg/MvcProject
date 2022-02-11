using CSG.Models.Entities;
using CSG.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CSG.Data
{
    public class GizemContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public GizemContext(DbContextOptions<GizemContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUserRequest>().HasKey(aur => new { aur.UserId, aur.RequestId });
            modelBuilder.Entity<ProductRequest>().HasKey(aur => new { aur.ProductId, aur.RequestId });

        }

        public DbSet<Request> Requests { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ApplicationUserRequest> ApplicationUserRequests { get; set; }
        public DbSet<ProductRequest> ProductRequests { get; set; }



    }
}
