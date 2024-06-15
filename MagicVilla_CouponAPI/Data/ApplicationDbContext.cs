using MagicVilla_CouponAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_CouponAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Default constructor to pass options to default class
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { 


        }

        public DbSet<Coupon> Coupons { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Coupon>().HasData(
                new Coupon { Id = 1, Name = "100OFF", Percent = 10, IsActive = true },
                new Coupon { Id = 2, Name = "200OFF", Percent = 20, IsActive = false }
            );
        }

    }
}
