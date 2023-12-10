using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MUANGAY.Models
{
    public class ChonPL
    {
        public IEnumerable<SANPHAM> spdata { get; set; }
        public IEnumerable<PHANLOAI> pldata { get; set; }
        public IEnumerable<PHANLOAICHITIET> plctdata { get; set; }
        public IEnumerable<THUONGHIEU> thdata { get; set; }

    }
}