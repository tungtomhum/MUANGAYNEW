using MUANGAY.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MUANGAY.Controllers
{
    public class UserController : Controller
    {
        private string connection;
        private dbMuaNgayDataContext db;

        public UserController()
        {
            // Khởi tạo chuỗi kết nối
            db = Connect.GetConnect();
        }
        // GET: User
        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangKy(FormCollection form)
        {
            // Retrieve values from FormCollection
            var hoTen = form["fHoTen"];
            var tenDN = form["fTenDN"];
            var matKhau = form["fMatKhau"];
            var email = form["fEmail"];
            var diaChi = form["fDiaChi"];
            var soDienThoai = form["fSDT"];
            var ngaySinh = DateTime.Parse(form["fNgaySinh"]);

            // Create a new KHACHHANG object
            var kh = new KHACHHANG
            {
                HoTen = hoTen,
                TaiKhoan = tenDN,
                MatKhau = matKhau,
                Email = email,
                DiaChi = diaChi,
                SoDienThoai = soDienThoai,
                NgaySinh = ngaySinh
            };

            // Add your logic for checking existing email and username
            if (db.KHACHHANGs.Any(n => n.Email == email) || db.KHACHHANGs.Any(n => n.TaiKhoan == tenDN))
            {
                ModelState.AddModelError("Email", "Email hoặc Tên đăng nhập đã tồn tại");
                return View();
            }

            // Insert data into the database
            db.KHACHHANGs.InsertOnSubmit(kh);
            db.SubmitChanges();

            return RedirectToAction("DangNhap");
        }






        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(string txtTendangnhap, string txtMatKhau)
        {
            if (String.IsNullOrEmpty(txtTendangnhap))
            {
                ViewData["Err1"] = "Bạn chưa nhập tên đăng nhập";
            }
            else if (String.IsNullOrEmpty(txtMatKhau))
            {
                ViewData["Err2"] = "Phải nhập mật khẩu";
            }
            else
            {
                if (txtTendangnhap == "admin" && txtMatKhau == "admin") // Replace with actual admin credentials
                {
                    // Admin login successful
                    Session["Admin"] = "admin"; // Save Admin login information to Session
                    return RedirectToAction("Index", "Admin", new { area = "Admin" });
                }
                else
                {
                    KHACHHANG kh = db.KHACHHANGs.SingleOrDefault(n => n.TaiKhoan == txtTendangnhap && n.MatKhau == txtMatKhau);
                    if (kh != null)
                    {
                        ViewBag.ThongBao = "Chúc mừng đăng nhập thành công";
                        Session["TaiKhoan"] = kh;

                        // Check shopping cart
                        var gioHang = Session["GioHang"] as List<GioHang>;
                        if (gioHang != null && gioHang.Any())
                        {
                            // Non-empty cart, redirect to DatHang page
                            return RedirectToAction("DatHang", "GioHang");
                        }
                        else
                        {
                            // Empty cart, redirect to Index page
                            return RedirectToAction("Home", "MUANGAY");
                        }
                    }
                    else
                    {
                        ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
                    }
                }
            }

            return View();
        }



        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("Home", "MUANGAY");
        }

        public ActionResult HoSo()
        {
            // Retrieve user information from the session
            KHACHHANG kh = Session["TaiKhoan"] as KHACHHANG;

            if (kh != null)
            {
                // Fetch the updated user information from the database
                var updatedKh = db.KHACHHANGs.SingleOrDefault(n => n.MaKH == kh.MaKH);

                if (updatedKh != null)
                {
                    // Update the session with the new information
                    Session["TaiKhoan"] = updatedKh;

                    // Pass the updated user information to the view
                    return View(updatedKh);
                }
            }

            return RedirectToAction("DangNhap", "User");
        }

        [HttpGet]
        public ActionResult EditKH(int id)
        {
            var sp = db.KHACHHANGs.SingleOrDefault(n => n.MaKH == id);
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditKH(FormCollection f)
        {
            var kh = db.KHACHHANGs.SingleOrDefault(n => n.MaKH == int.Parse(f["fmakh"]));

            if (ModelState.IsValid)
            {

                kh.HoTen = f["fhoten"];
                kh.Email = f["femail"];
                kh.SoDienThoai = f["fsdt"];
                kh.DiaChi = f["fdiachi"];
                kh.TaiKhoan = f["ftaikhoan"];
                kh.NgaySinh = Convert.ToDateTime(f["fngaysinh"]);

                db.SubmitChanges();
                //Vé trang QL SACH
                return RedirectToAction("HoSo");
            }
            return View(kh);
        }
        [HttpGet]
        public ActionResult EditMK(int id)
        {
            var sp = db.KHACHHANGs.SingleOrDefault(n => n.MaKH == id);
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditMK(FormCollection f)
        {
            var kh = db.KHACHHANGs.SingleOrDefault(n => n.MaKH == int.Parse(f["fmakh"]));

            if (ModelState.IsValid)
            {

                kh.MatKhau = f["fmk"];             

                db.SubmitChanges();
                //Vé trang QL SACH
                return RedirectToAction("HoSo");
            }
            return View(kh);
        }
    }
}