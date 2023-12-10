using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MUANGAY.Models
{
    public class phanhoi
    {
        public int ReviewCount { get; set; }
        public IEnumerable<PHANHOI> phdata { get; set; }
        public IEnumerable<SANPHAM> spdata { get; set; }
    }
}