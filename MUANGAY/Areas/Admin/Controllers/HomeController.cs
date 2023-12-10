using MUANGAY.Controllers;
using MUANGAY.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MUANGAY.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private dbMuaNgayDataContext data;

        public HomeController()
        {
            data = Connect.GetConnect();
        }
        // GET: Admin/Home
        public ActionResult Home()
        {
            // Check if the user is logged in
            if (Session["Admin"] is ADMIN admin)
            {
                // Get counts from the database based on the logged-in user's MaQuanLy
                int DemDDH = data.DONHANGs.Count();
                int TongDT = data.DONHANGs
                    .Where(sc => sc.ThanhToan == true)
                    .Sum(sc => sc.TongTien ?? 0); // Use the null-coalescing operator to handle null values

                // Pass counts to the view
                ViewBag.DemDDH = DemDDH;
                ViewBag.TongDT = TongDT;

                return View();
            }

            // If the user is not logged in, you might want to redirect them to the login page or handle it accordingly.
            return RedirectToAction("Login", "Home");
        }

        public ActionResult NavPartial()
        {
            ADMIN admin = Session["Admin"] as ADMIN;
            if (admin != null)
            {
                return View();
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Login()
        {
            // No need for authentication check on the login page
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection form)
        {
            var username = form["username"];
            var password = form["password"];

            ADMIN admin = data.ADMINs.SingleOrDefault(n => n.Username == username && n.Password == password);
            if (admin != null)
            {
                Session["Admin"] = admin;

                return RedirectToAction("Home", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng";
                ModelState.AddModelError(string.Empty, "Invalid username or password");
                return View();
            }
        }

        public ActionResult HoSo()
        {
            // Retrieve user information from the session
            ADMIN ad = Session["Admin"] as ADMIN;
            if (ad != null)
            {
                // Fetch the updated user information from the database
                var updatedKh = data.ADMINs.SingleOrDefault(n => n.MaAdmin == ad.MaAdmin);

                if (updatedKh != null)
                {
                    // Update the session with the new information
                    Session["Admin"] = updatedKh;

                    // Pass the updated user information to the view
                    return View(updatedKh);
                }
            }

            return RedirectToAction("Login");
        }
        [HttpGet]
        public ActionResult EditMK(string id)
        {
            var sp = data.ADMINs.SingleOrDefault(n => n.MaAdmin == id);
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
            var ad = data.ADMINs.SingleOrDefault(n => n.MaAdmin == f["fmaad"]);

            if (ModelState.IsValid)
            {

                ad.Password = f["fmk"];

                data.SubmitChanges();
                //Vé trang QL SACH
                return RedirectToAction("HoSo");
            }
            return View(ad);
        }
        public ActionResult QuanLySanPham()
        {
            ADMIN admin = Session["Admin"] as ADMIN;

            var sp = data.SANPHAMs.ToList();
            // Check for success message in TempData
            ViewBag.SuccessMessage = TempData["SuccessMessage"] as string;

            return View(sp.OrderBy(n => n.NgayCapNhat));
        }
        public ActionResult QuanLyKhachHang()
        {
            ADMIN admin = Session["Admin"] as ADMIN;

            var kh = data.KHACHHANGs.ToList();
            // Check for success message in TempData
            ViewBag.SuccessMessage = TempData["SuccessMessage"] as string;

            return View(kh.OrderBy(n => n.HoTen));
        }
        public ActionResult QuanLyDonHang()
        {
            ADMIN admin = Session["Admin"] as ADMIN;

            var dh = data.DONHANGs.ToList();
            // Check for success message in TempData
            ViewBag.SuccessMessage = TempData["SuccessMessage"] as string;

            return View(dh.OrderBy(n => n.NgayDatHang));
        }
        public ActionResult QuanLyPhanHoi()
        {
            ADMIN admin = Session["Admin"] as ADMIN;

            var ph = data.PHANHOIs.ToList();
            // Check for success message in TempData
            ViewBag.SuccessMessage = TempData["SuccessMessage"] as string;

            return View(ph.OrderBy(n => n.NgayGui));
        }









        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        private bool IsAdminLoggedIn()
        {
            return Session["Admin"] != null;
        }

        private ActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Home");
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // Check if the action is not the Login action before redirecting
            if (!IsAdminLoggedIn() && filterContext.ActionDescriptor.ActionName != "Login")
            {
                filterContext.Result = RedirectToLogin();
            }
        }
        [HttpPost]
        public ActionResult DeleteSP(int masp)
        {
            if (Session["Admin"] is ADMIN admin)
            {
                var sanPhamToDelete = data.SANPHAMs.SingleOrDefault(sp => sp.MaSP == masp);

                if (sanPhamToDelete != null)
                {
                    var isProductInOrder = data.CHITIETDATHANGs.Any(ct => ct.MaSP == masp);

                    if (isProductInOrder)
                    {
                        ViewBag.ErrorMessage = "Sản phẩm đang tồn tại trong đơn hàng!";
                        return View("QuanLySanPham", data.SANPHAMs.OrderByDescending(a => a.NgayCapNhat).ToList());
                    }

                    data.SANPHAMs.DeleteOnSubmit(sanPhamToDelete);
                    data.SubmitChanges();

                    TempData["SuccessMessage"] = "Xóa sản phẩm thành công";

                    ViewBag.SuccessMessage = "Xóa sản phẩm thành công";

                    return RedirectToAction("QuanLySanPham");
                }
                else
                {
                    ViewBag.ErrorMessage = "Product not found.";
                    return View("QuanLySanPham");
                }
            }
            return RedirectToAction("Login");
        }
        [HttpPost]
        public ActionResult DeleteKH(int makh)
        {
            if (Session["Admin"] is ADMIN admin)
            {
                var khToDelete = data.KHACHHANGs.SingleOrDefault(sp => sp.MaKH == makh);

                if (khToDelete != null)
                {
                    var isProductInOrder = data.DONHANGs.Any(ct => ct.MaKH == makh);

                    if (isProductInOrder)
                    {
                        ViewBag.ErrorMessage = "Khách hàng còn đơn hàng chưa thanh toán!";
                        return View("QuanLyKhachHang", data.KHACHHANGs.OrderByDescending(a => a.HoTen).ToList());
                    }

                    data.KHACHHANGs.DeleteOnSubmit(khToDelete);
                    data.SubmitChanges();

                    TempData["SuccessMessage"] = "Xóa thông tin Khách hàng thành công";

                    ViewBag.SuccessMessage = "Xóa thông tin Khách hàng thành công";

                    return RedirectToAction("QuanLyKhachHang");
                }
                else
                {
                    ViewBag.ErrorMessage = "Product not found.";
                    return View("QuanLyKhachHang");
                }
            }
            return RedirectToAction("Login");
        }
        [HttpPost]
        public ActionResult DeleteDH(int madh)
        {
            if (Session["Admin"] is ADMIN admin)
            {
                var dhToDelete = data.DONHANGs.SingleOrDefault(sp => sp.MaDH == madh);

                if (dhToDelete != null)
                {
                    // Kiểm tra xem đơn hàng đã được thanh toán hay chưa
                    if (dhToDelete.ThanhToan == true)
                    {
                        // Xóa các hàng trong bảng CHITIETDATHANG theo MaDH
                        var chiTietDatHangList = data.CHITIETDATHANGs.Where(ct => ct.MaDH == madh).ToList();
                        data.CHITIETDATHANGs.DeleteAllOnSubmit(chiTietDatHangList);

                        // Xóa đơn hàng
                        data.DONHANGs.DeleteOnSubmit(dhToDelete);

                        data.SubmitChanges();

                        TempData["SuccessMessage"] = "Xóa thông tin đơn hàng thành công";

                        return RedirectToAction("QuanLyDonHang");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Đơn hàng chưa được thanh toán!";
                        return View("QuanLyDonHang", data.DONHANGs.OrderByDescending(a => a.NgayDatHang).ToList());
                    }
                }
                else
                {
                    return RedirectToAction("QuanLyDonHang");
                }
            }

            return RedirectToAction("Login");
        }


        [HttpGet]
        public ActionResult EditSP(int id)
        {
            var sp = data.SANPHAMs.SingleOrDefault(n => n.MaSP == id);
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            // Gán danh sách cho ViewBag
            ViewBag.MaPL = new SelectList(data.PHANLOAIs.ToList().OrderBy(n => n.TenLoai), "MaPL", "TenLoai", sp.MaPL);
            ViewBag.MaPLCT = new SelectList(data.PHANLOAICHITIETs.ToList().OrderBy(n => n.TenPLCT), "MaPLCT", "TenPLCT", sp.MaPLCT);
            ViewBag.MaThuongHieu = new SelectList(data.THUONGHIEUs.ToList().OrderBy(n => n.TenThuongHieu), "MaThuongHieu", "TenThuongHieu", sp.MaThuongHieu);

            return View(sp);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditSP(FormCollection f, HttpPostedFileBase fFileUpload)
        {
            var sp = data.SANPHAMs.SingleOrDefault(n => n.MaSP == int.Parse(f["fmasp"]));
            ViewBag.MaPL = new SelectList(data.PHANLOAIs.ToList().OrderBy(n => n.TenLoai), "MaPL", "TenLoai", sp.MaPL);
            ViewBag.MaPLCT = new SelectList(data.PHANLOAICHITIETs.ToList().OrderBy(n => n.TenPLCT), "MaPLCT", "TenPLCT", sp.MaPLCT);
            ViewBag.MaThuongHieu = new SelectList(data.THUONGHIEUs.ToList().OrderBy(n => n.TenThuongHieu), "MaThuongHieu", "TenThuongHieu", sp.MaThuongHieu);

            if (ModelState.IsValid)
            {
                if (fFileUpload != null)
                {
                    var sFileName = Path.GetFileName(fFileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/img"), sFileName);

                    if (!System.IO.File.Exists(path))
                    {
                        fFileUpload.SaveAs(path);
                    }
                    sp.AnhSP = sFileName;
                }

                sp.TenSanPham = f["ftensp"];
                sp.KichThuoc = f["fkt"];
                sp.MoTaSP = f["fmota"];
               
                sp.NgayCapNhat = DateTime.Now;
                sp.SoLuong = int.Parse(f["fsl"]);
                sp.GiaTien = decimal.Parse(f["fgia"]);

                data.SubmitChanges();
                //Vé trang QL SACH
                return RedirectToAction("QuanLySanPham");
            }
            return View(sp);
        }
        [HttpGet]
        public ActionResult AddSP()
        {
            var viewModel = new ChonPL
            {
                pldata = data.PHANLOAIs.ToList(),
                plctdata = data.PHANLOAICHITIETs.ToList(),
                thdata = data.THUONGHIEUs.ToList()
            };

            return View(viewModel);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddSP(SANPHAM sp, FormCollection f, HttpPostedFileBase fFileUpload)
        {
            if (ModelState.IsValid)
            {
                if (fFileUpload != null && fFileUpload.ContentLength > 0)
                {
                    var sFileName = Path.GetFileName(fFileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/img"), sFileName);

                    if (!System.IO.File.Exists(path))
                    {
                        fFileUpload.SaveAs(path);
                    }

                    sp.TenSanPham = f["ftensp"];
                    sp.KichThuoc = f["fkt"];
                    sp.MoTaSP = f["fmota"];
                    sp.AnhSP = sFileName;
                    sp.GiaTien = Convert.ToInt32(f["fgia"]);
                    sp.MaPL = f["fmapl"];
                    sp.MaPLCT = f["fmaplct"];
                    sp.MaThuongHieu = f["fmath"];
                    sp.NgayCapNhat = DateTime.Now;
                    sp.SoLuong = Convert.ToInt32(f["fsl"]);

                    // Assuming 'data' is your DataContext or DbContext
                    data.SANPHAMs.InsertOnSubmit(sp);
                    data.SubmitChanges();

                    // Redirect to the product management page
                    return RedirectToAction("QuanLySanPham", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Please upload an image for the product.");
                }
            }

            // If ModelState is not valid, return to the AddSP view with the existing model
            var viewModel = new ChonPL
            {
                pldata = data.PHANLOAIs.ToList(),
                plctdata = data.PHANLOAICHITIETs.ToList(),
                thdata = data.THUONGHIEUs.ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult EditKH(int id)
        {
            var sp = data.KHACHHANGs.SingleOrDefault(n => n.MaKH == id);
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
            var kh = data.KHACHHANGs.SingleOrDefault(n => n.MaKH == int.Parse(f["fmakh"]));

            if (ModelState.IsValid)
            {

                kh.HoTen = f["ftenkh"];
                kh.Email = f["femail"];
                kh.SoDienThoai = f["fsdt"];
                kh.DiaChi = f["fdiachi"];
                kh.TaiKhoan = f["ftaikhoan"];
                kh.NgaySinh = Convert.ToDateTime(f["fngaysinh"]);

                data.SubmitChanges();
                //Vé trang QL SACH
                return RedirectToAction("QuanLyKhachHang");
            }
            return View(kh);
        }
        [HttpGet]
        public ActionResult EditDH(int id)
        {
            var dh = data.DONHANGs.SingleOrDefault(n => n.MaDH == id);
            if (dh == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(dh);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditDH(FormCollection f)
        {
            var dh = data.DONHANGs.SingleOrDefault(n => n.MaDH == int.Parse(f["fmadh"]));

            if (ModelState.IsValid)
            {

                dh.NgayGiaoHang= DateTime.Now;
                dh.ThanhToan = Convert.ToBoolean(f["ftt"]);
                dh.TrangThai = "Thanh toán thành công";


                data.SubmitChanges();
                //Vé trang QL SACH
                return RedirectToAction("QuanLyDonHang");
            }
            return View(dh);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult PhanHoiKH(FormCollection f)
        {
            var dh = data.PHANHOIs.SingleOrDefault(n => n.MaPhanHoi == int.Parse(f["maph"]));

            if (ModelState.IsValid)
            {

                dh.NgayPHLai = DateTime.Now;
                dh.NguoiPH = "Admin";
                dh.NDPHLai = f["fndphkh"];

                data.SubmitChanges();
                //Vé trang QL SACH
                return RedirectToAction("QuanLyPhanHoi");
            }
            return View(dh);
        }
    }
}