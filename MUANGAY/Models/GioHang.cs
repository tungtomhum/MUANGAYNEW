using MUANGAY.Controllers;
using MUANGAY.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MUANGAY.Models
{
    public class GioHang
    {
        private string connection;
        private dbMuaNgayDataContext db = Connect.GetConnect();

 
        public int iMaSP { get; set; }
        public string sTenSanPham { get; set; }
        public string sAnhSP { get; set; }
        public double dDonGia { get; set; }
        public int iSoLuong { get; set; }
        public double dThanhTien { get { return iSoLuong * dDonGia; } }

        public GioHang(int ms)
        {
            iMaSP = ms;

            // Kiểm tra nếu db là null thì khởi tạo lại
            if (db == null)
            {
                db = Connect.GetConnect();
            }

            SANPHAM s = db.SANPHAMs.Single(n => n.MaSP == iMaSP);
            sTenSanPham = s.TenSanPham;
            sAnhSP = s.AnhSP;
            dDonGia = double.Parse(s.GiaTien.ToString());
            iSoLuong = 1;
        }
    }

}