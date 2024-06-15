using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MagicVilla_CouponAPI.Repository
{
    public class CouponRepository : ICouponRepository
    {
        private readonly ApplicationDbContext _db;

        public CouponRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task CreateAsync(Coupon coupon)
        {
          await  _db.AddAsync(coupon);
        }

        public async Task<ICollection<Coupon>> GetAllAsync()
        {
            return await _db.Coupons.ToListAsync();
        }

        public async Task<Coupon> GetAsync(string couponName)
        {
            return await _db.Coupons.FirstOrDefaultAsync(c => c.Name.ToLower() == couponName.ToLower());
        }

        public async Task<Coupon> GetAsync(int id)
        {
            return await _db.Coupons.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task RemoveAsync(Coupon coupon)
        {
            _db.Remove(coupon);
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Coupon coupon)
        {
            _db.Coupons.Update(coupon);
        }
    }
}
