using MUANGAY.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace MUANGAY.Controllers
{
    public class MUANGAYController : Controller
    {
        private string connection;
        private dbMuaNgayDataContext db;

        public MUANGAYController()
        {
            // Khởi tạo chuỗi kết nối
            db = Connect.GetConnect();
        }
        public ActionResult NavPartial()
        {
            return PartialView();
        }
        public ActionResult FooterPartial()
        {
            return PartialView();
        }

        public ActionResult LoginLogout()
        {
            return PartialView("LoginLogoutPartial"); 
        }
        public ActionResult BannerPartial()
        {
            return PartialView();
        }

        private List<BANNER> LayAnhBanner(int count, string maPL)
        {
            // Thay thế điều kiện lọc theo MaPL
            return db.BANNERs.Where(b => b.MaPL == maPL).Take(count).ToList();
        }

        public ActionResult BannerSPPartial(string maPL)
        {
            var listanh = LayAnhBanner(10, maPL);
            return PartialView(listanh);
        }


        public ActionResult PLCTPartial()
        {
            var phanloaichitiets = db.PHANLOAICHITIETs.ToList().OrderByDescending(n => n.MaPL).ToList();

            foreach (var plct in phanloaichitiets)
            {
                ViewData["SoLuongSanPham_" + plct.MaPLCT] = DemSoLuongSanPhamTheoMaPLCT(plct.MaPLCT);
            }

            return PartialView(phanloaichitiets);
        }
        private int DemSoLuongSanPhamTheoMaPLCT(string maPLCT)
        {
            // Sử dụng Entity Framework để thực hiện truy vấn
            var soLuong = db.SANPHAMs.Count(s => s.MaPLCT == maPLCT);
            return soLuong;
        }
        [HttpGet]
        public ActionResult LocPLCT(string maPLCT, int? page)
        {
            ViewBag.MaPLCT = maPLCT;
            ViewBag.TenPLCT = db.PHANLOAICHITIETs.FirstOrDefault(plct => plct.MaPLCT == maPLCT)?.TenPLCT;

            var sanPhamTheoDanhMuc = db.SANPHAMs
                .Where(sp => sp.MaPLCT == maPLCT)
                .OrderByDescending(sp => sp.NgayCapNhat)
                .ToList();

            return View("LocPLCT", sanPhamTheoDanhMuc);
        }


        [HttpPost]
        public ActionResult LocPLCT(FormCollection form)
        {
            string maPLCT = form["maPLCT"];
            return RedirectToAction("LocPLCT", new { maPLCT });
        }


        // GET: MUANGAY
        public ActionResult Home(int? page)
        {
            int pageSize = 12;
            int pageNumber = page ?? 1;

            var allSP = db.SANPHAMs.OrderByDescending(a => a.NgayCapNhat);

            var pagedSP = allSP.ToPagedList(pageNumber, pageSize);

            return View(pagedSP);
        }
        public ActionResult ChiTietSP(int id)
        {
            var viewModel = new phanhoi
            {
                phdata = db.PHANHOIs.Where(p => p.MaSP == id).ToList(),
                spdata = db.SANPHAMs.Where(s => s.MaSP == id).ToList()
            };

            viewModel.ReviewCount = viewModel.phdata.Count();

            return View(viewModel);
        }






        //PHAN LOAI TRANG THEO SANPHAM
        public ActionResult SanPham(string maPL, int? page)
        {
            int pageSize = 6;
            int pageNumber = page ?? 1;

            var allSP = db.SANPHAMs
                .Where(s => s.PHANLOAI.MaPL == maPL) 
                .OrderByDescending(a => a.NgayCapNhat);

            var pagedSP = allSP.ToPagedList(pageNumber, pageSize);

            ViewBag.MaPL = maPL;

            return View(pagedSP);
        }


        public ActionResult LOCPLCTPartial(string maPL)
        {
            var listTT = (from cd in db.PHANLOAICHITIETs
                          where cd.MaPL == maPL
                          orderby db.SANPHAMs.Count(sp => sp.MaPLCT == cd.MaPLCT) descending
                          select cd)
                          .ToList();

            foreach (var plct in listTT)
            {
                ViewData["SoLuongSanPham_" + plct.MaPLCT] = DemSoLuongSanPhamTheoMaPLCT(plct.MaPLCT);
            }

            return PartialView(listTT);
        }
        private string LayTenPL(string id)
        {
            var tt = db.PHANLOAIs.SingleOrDefault(cd => cd.MaPL == id);

            if (tt != null)
            {
                return tt.TenLoai;
            }

            return "Không tìm thấy Loại";
        }
        public ActionResult PhanLoaiSP(string maPL, string id, string tenPLCT, int? page)
        {
            int pageSize = 6;
            int pageNumber = page ?? 1;

            var sp = db.SANPHAMs.Where(s => s.MaPLCT == id)
                                  .OrderByDescending(s => s.NgayCapNhat)
                                  .ToPagedList(pageNumber, pageSize);

            ViewBag.Id = id;
            ViewBag.ten = LayTenPL(id);
            ViewBag.TenPLCT = tenPLCT;

            return View(sp);
        }


        //LOC THEO GIA TIEN
        public ActionResult ChonTienPartial()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult SanPhamTheoGia(string id, int? page, int minPrice, int maxPrice)
        {
            // Assuming SANPHAM has a property named GiaTien
            var sp = db.SANPHAMs
                .Where(s => s.GiaTien >= minPrice && s.GiaTien <= maxPrice)
                .OrderByDescending(s => s.GiaTien);

            ViewBag.Id = id;
            ViewBag.ten = LayTenPL(id);
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(sp); // Assuming the view name is "SanPhamTheoGia.cshtml"
        }
        [HttpPost]
        public ActionResult GuiPhanHoi(string phanhoi, string email)
        {
            // Validate and save the feedback and email to the database
            if (!string.IsNullOrEmpty(phanhoi) && !string.IsNullOrEmpty(email))
            {
                var phanhoiEntity = new PHANHOI
                {
                    NoiDung = phanhoi,
                    Email = email,
                    NgayGui = DateTime.Now // You may need to adjust this based on your database schema
                };

                db.PHANHOIs.InsertOnSubmit(phanhoiEntity);
                db.SubmitChanges(); // Save changes to the database

                // Optionally, you can redirect the user to a success page or perform other actions
                return RedirectToAction("Home");
            }

            // If validation fails, you may want to handle the error accordingly
            return RedirectToAction("Error");
        }
        [HttpPost]
        public ActionResult GuiPhanHoiSP(string phanhoi, string email, string hoten, int masp)
        {
            // Validate and save the feedback and email to the database
            if (!string.IsNullOrEmpty(phanhoi) && !string.IsNullOrEmpty(email))
            {
                var phanhoiEntity = new PHANHOI
                {
                    NoiDung = phanhoi,
                    Email = email,
                    NgayGui = DateTime.Now,
                    HoTen = hoten,
                    MaSP = masp
                };

                db.PHANHOIs.InsertOnSubmit(phanhoiEntity);
                db.SubmitChanges(); // Save changes to the database

                // Optionally, you can redirect the user to a success page or perform other actions
                return RedirectToAction("ChiTietSP", new { id = masp });
            }

            // If validation fails, you may want to handle the error accordingly
            return RedirectToAction("Error");
        }
        [HttpGet]
        public ActionResult TimKiem(string tuKhoa, int? page)
        {
            ViewBag.tuKhoa = tuKhoa;
            int pageSize = 20;
            int pageNumber = (page ?? 1);

            var sachTimKiem = db.SANPHAMs
                .Where(s => s.TenSanPham.Contains(tuKhoa))
                .OrderByDescending(s => s.NgayCapNhat)
                .ToPagedList(pageNumber, pageSize);

            return View(sachTimKiem);
        }


        [HttpPost]
        public ActionResult TimKiem(FormCollection form)
        {
            string tuKhoa = form["tuKhoa"];
            ViewBag.TuKhoa = tuKhoa; // Lưu từ khóa để hiển thị trong view
            return RedirectToAction("TimKiem", new { tuKhoa });
        }
    }
}