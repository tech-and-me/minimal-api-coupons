﻿namespace MagicVilla_CouponAPI.Models.DTO
{
    public class CouponUpdateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Percent { get; set; }
        public bool IsActive { get; set; }
    }
}
