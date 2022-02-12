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

            modelBuilder.Entity<ApplicationUserRequest>().HasKey(aur => new { aur.ApplicationUserId, aur.RequestId });
            modelBuilder.Entity<ApplicationUserRequest>()
                .HasOne(aur => aur.ApplicationUser)
                .WithMany(au => au.ApplicationUserRequests)
                .HasForeignKey(aur => aur.ApplicationUserId);
            modelBuilder.Entity<ApplicationUserRequest>()
                .HasOne(aur => aur.Request)
                .WithMany(r => r.ApplicationUserRequests)
                .HasForeignKey(aur => aur.RequestId);

            modelBuilder.Entity<ProductRequest>().HasKey(aur => new 
                { aur.ProductId, aur.RequestId });
            modelBuilder.Entity<ProductRequest>()
                .HasOne(pr => pr.Product)
                .WithMany(p => p.ProductRequests)
                .HasForeignKey(pr => pr.ProductId);
            modelBuilder.Entity<ProductRequest>()
                .HasOne(pr => pr.Request)
                .WithMany(r => r.ProductRequests)
                .HasForeignKey(pr => pr.RequestId);

        }

        public DbSet<Request> Requests { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ApplicationUserRequest> ApplicationUserRequests { get; set; }
        public DbSet<ProductRequest> ProductRequests { get; set; }



    }
}
