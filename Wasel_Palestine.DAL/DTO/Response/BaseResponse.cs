using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.DTO.Response
{
    public class BaseResponse
    {
            public bool Success { get; set; }
            public string Message { get; set; }
        public List<string>? Errors { get; set; }
    }
}
