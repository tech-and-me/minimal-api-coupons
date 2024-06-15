using MagicVilla_CouponAPI.Models;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MagicVilla_CouponAPI.Repository.IRepository
{
    public interface ICouponRepository
    {
        Task<ICollection<Coupon>> GetAllAsync();
        Task<Coupon> GetAsync(string couponName);
        Task<Coupon> GetAsync(int id);
        Task CreateAsync(Coupon coupon);
        Task UpdateAsync(Coupon coupon);
        Task RemoveAsync(Coupon coupon);
        Task SaveAsync();

    }
}
