using System;
using System.Collections.Generic;
using System.Net;

namespace MagicVilla_CouponAPI.Models
{
    public class APIResponse
    {
        public bool IsSuccess { get; set; }
        public Object Result { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<string> ErrorMessages { get; set; }
        public APIResponse()
        {
            ErrorMessages = new List<string>();
        }
        
    }
}
