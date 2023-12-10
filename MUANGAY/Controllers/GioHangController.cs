using MUANGAY.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Mvc;
using VNPAY_CS_ASPX;

namespace MUANGAY.Controllers
{
    public class GioHangController : Controller
    {
        private string connection;
        private dbMuaNgayDataContext db;

        public GioHangController()
        {
            // Khởi tạo chuỗi kết nối
            db = Connect.GetConnect();
        }
        public List<GioHang> LayGioHang()
        {
            var gioHang = Session["GioHang"] as List<GioHang>;
            if (Session["GioHang"] == null)
            {
                gioHang = new List<GioHang>();
                Session["GioHang"] = gioHang;
            }
            else
            {
                gioHang = Session["GioHang"] as List<GioHang>;
            }

            return gioHang;
        }

        // ThemGioHang
        [HttpPost]
        public ActionResult ThemGioHangtt(int ms, string url)
        {
            var gioHang = LayGioHang();
            var sanPham = gioHang.Find(n => n.iMaSP == ms);
            if (sanPham == null)
            {
                sanPham = new GioHang(ms);
                gioHang.Add(sanPham);
            }
            else
            {
                sanPham.iSoLuong++;
            }

            // Save the updated cart to the session or database, depending on your implementation

            // Return a JSON response indicating success
            return Json(new { success = true });
        }

        public ActionResult ThemGioHang(int ms, string url, FormCollection f)
        {
            int sl = int.Parse(f["sl"]);
            var gioHang = LayGioHang();
            var sanPham = gioHang.Find(n => n.iMaSP == ms);
            if (sanPham == null)
            {
                sanPham = new GioHang(ms);
                gioHang.Add(sanPham);
                sanPham.iSoLuong = sl;
            }
            else
            {
                sanPham.iSoLuong += sl;
            }
            return Redirect(url);
        }

        // TongSoLuong
        private int TongSoLuong(List<GioHang> gioHang)
        {
            return gioHang.Sum(n => n.iSoLuong);
        }

        // TinhTongTien
        private double TongTien()
        {
            double tt = 0;
            List<GioHang> l = Session["GioHang"] as List<GioHang>;
            if (l!=null)
            {
                tt = l.Sum(n => n.dThanhTien);
            }
            return tt;
        }

        // GioHang
        public ActionResult GioHang()
        {
            var gioHang = LayGioHang();
            if (gioHang.Count == 0)
            {
                return RedirectToAction("Home", "MUANGAY");
            }

            ViewBag.TongSoLuong = TongSoLuong(gioHang);
            ViewBag.TongTien = TongTien();
            return View(gioHang);
        }

        // GioHangPartial
        public ActionResult GioHangPartial()
        {
            var gioHang = LayGioHang();
            ViewBag.TongSoLuong = TongSoLuong(gioHang);
            ViewBag.TongTien = TongTien();
            return PartialView();
        }
        // XoaSPKhoiGioHang
        public ActionResult XoaSPKhoiGioHang(int iMaSach)
        {
            var gioHang = LayGioHang();
            var sanPham = gioHang.SingleOrDefault(n => n.iMaSP == iMaSach);
            if (sanPham != null)
            {
                gioHang.Remove(sanPham);
                if (gioHang.Count == 0)
                {
                    return RedirectToAction("Home", "MUANGAY");
                }
            }
            return RedirectToAction("GioHang");
        }

        // XoaGioHang
        public ActionResult XoaGioHang()
        {
            var gioHang = LayGioHang();
            gioHang.Clear();
            return RedirectToAction("Home", "MUANGAY");
        }
        // CapNhatGioHang
        [HttpPost]
        public ActionResult CapNhatGioHang(int iMaSach, FormCollection f)
        {
            var gioHang = LayGioHang();
            var sanPham = gioHang.SingleOrDefault(n => n.iMaSP == iMaSach);
            if (sanPham != null)
            {
                sanPham.iSoLuong = int.Parse(f["txtSoLuong"].ToString());
            }
            return RedirectToAction("GioHang");
        }
        [HttpGet]
        public ActionResult DatHang()
        {
            if (Session["TaiKhoan"] == null || Session["TaiKhoan"].ToString() == "")
            {
                return RedirectToAction("DangNhap", "User");
            }
            if (Session["GioHang"] == null)
            {
                return RedirectToAction("Index", "SachOnline");
            }
            List<GioHang> lstGioHang = LayGioHang();
            ViewBag.TongSoLuong = TongSoLuong(lstGioHang);
            ViewBag.TongTien = TongTien();

            return View(lstGioHang);
        }

        [HttpPost]
        public ActionResult DatHang(FormCollection f)
        {
            if (Session["TaiKhoan"] != null && !string.IsNullOrEmpty(Session["TaiKhoan"].ToString()))
            {
                string paymentMethod = f["paymentMethod"];
                string paymentType = f["paymentType"];
                KHACHHANG kh = (KHACHHANG)Session["TaiKhoan"];
                DONHANG od = new DONHANG();
                List<GioHang> lst = LayGioHang();
                if (paymentMethod.Equals("ocd"))
                {
                    od.MaKH = kh.MaKH;
                    od.NgayDatHang = DateTime.Now;
                    od.ThanhToan = false;
                    od.TrangThai = "Thanh toán khi nhận hàng";
                    db.DONHANGs.InsertOnSubmit(od);
                    db.SubmitChanges();

                    foreach (var gioHangItem in lst)
                    {
                        CHITIETDATHANG odd = new CHITIETDATHANG();
                        odd.MaSP = gioHangItem.iMaSP;
                        odd.MaDH = od.MaDH;
                        odd.SoLuong = gioHangItem.iSoLuong;
                        odd.DonGia = (int)gioHangItem.dDonGia;

                        db.CHITIETDATHANGs.InsertOnSubmit(odd);
                        // Update SANPHAM table: Subtract the ordered quantity from the product's stock
                        SANPHAM sp = db.SANPHAMs.SingleOrDefault(p => p.MaSP == odd.MaSP);
                        if (sp != null)
                        {
                            sp.SoLuong -= odd.SoLuong;
                        }
                    }

                    od.TongTien = (int)TongTien();
                    db.SubmitChanges();
                    Session["GioHang"] = null;
                    return RedirectToAction("XacNhanDonHang", "GioHang");
                }
                else if (paymentMethod.Equals("banking"))
                {
                    od.MaKH = kh.MaKH;
                    od.NgayDatHang = DateTime.Now;
                    od.ThanhToan = false;
                    od.TrangThai = "Hủy thanh toán";
                    db.DONHANGs.InsertOnSubmit(od);
                    db.SubmitChanges();

                    foreach (var gioHangItem in lst)
                    {
                        CHITIETDATHANG odd = new CHITIETDATHANG();
                        odd.MaDH = od.MaDH;
                        odd.MaSP = gioHangItem.iMaSP;
                        odd.SoLuong = gioHangItem.iSoLuong;
                        odd.DonGia = (int)gioHangItem.dDonGia;

                        db.CHITIETDATHANGs.InsertOnSubmit(odd);
                    }
                }

                od.TongTien = (int)TongTien();
                db.SubmitChanges();
                Session["GioHang"] = null;
                string url = btnPay_Click(od.MaDH);
                return Redirect(url);
                
            }
            else
            {
                return RedirectToAction("TaiKhoan", "User");
            }
        }


        // XacNhanDonHang
        public ActionResult XacNhanDonHang()
        {
            ViewBag.Msg = "CHÚC MỪNG BẠN ĐÃ ĐẶT HÀNG THÀNH CÔNG";
            ViewBag.icon = "http://pluspng.com/img-png/success-png-png-svg-512.png";
            ViewBag.color = "rgb(65, 209, 161)";
            return View();
        }

        protected string btnPay_Click(int id)
        {
            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"];
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];

            //Get payment input
            var order = db.DONHANGs.FirstOrDefault(o => o.MaDH == id);
            //Save order to db

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)order.TongTien * 100).ToString());
         

            DateTime createDate = Convert.ToDateTime(order.NgayDatHang);
            vnpay.AddRequestData("vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + order.MaDH);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.MaDH.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return paymentUrl;
        }

        public ActionResult thanhcong()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }
                //vnp_TxnRef: Ma don hang merchant gui VNPAY tai command=pay    
                //vnp_TransactionNo: Ma GD tai he thong VNPAY
                //vnp_ResponseCode:Response code from VNPAY: 00: Thanh cong, Khac 00: Xem tai lieu
                //vnp_SecureHash: HmacSHA512 cua du lieu tra ve

                int orderId = Convert.ToInt32(vnpay.GetResponseData("vnp_TxnRef"));
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                String TerminalID = Request.QueryString["vnp_TmnCode"];
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                String bankCode = Request.QueryString["vnp_BankCode"];
                var od = db.DONHANGs.FirstOrDefault(n => n.MaDH == orderId);
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        //Thanh toán thành công
                        od.ThanhToan = true;
                        od.TrangThai = "Thanh toán thành công";
                        od.NgayGiaoHang = DateTime.Now;
                        db.SubmitChanges();
                        ViewBag.Msg = "CHÚC MỪNG BẠN ĐÃ THANH TOÁN THÀNH CÔNG";
                        ViewBag.icon = "http://pluspng.com/img-png/success-png-png-svg-512.png";
                        ViewBag.color = "rgb(65, 209, 161)";
                        ViewBag.totalPrice = "Số tiền thanh toán : " + vnp_Amount.ToString() + "VND";
                    }
                    else
                    {
                        db.DONHANGs.DeleteOnSubmit(od);
                        ViewBag.Msg = "Có lỗi xảy ra trong quá trình xử lý";
                        ViewBag.icon = "https://images.onlinelabels.com/images/clip-art/molumen/molumen_red_round_error_warning_icon.png";
                        ViewBag.color = "red";
                        ViewBag.totalPrice = "Số tiền thanh toán : 0 VND";
                    }
                    ViewBag.IDweb = "Mã Website (Terminal ID) : " + TerminalID;
                    ViewBag.SymbolsPayment = "Mã giao dịch thanh toán : " + orderId.ToString();
                    ViewBag.SymbolsVNPAY = "Mã giao dịch tại VNPAY : " + vnpayTranId.ToString();
                    ViewBag.bank = "Ngân hàng thanh toán : " + bankCode;
                }
                else
                {
                    db.DONHANGs.DeleteOnSubmit(od);
                    ViewBag.Msg = "Có lỗi xảy ra trong quá trình xử lý";
                    ViewBag.icon = "https://images.onlinelabels.com/images/clip-art/molumen/molumen_red_round_error_warning_icon.png";
                    ViewBag.color = "red";
                }
            }
            return View();
        }

        public ActionResult LichSuDonHang()
        {
            if (Session["TaiKhoan"] != null && !string.IsNullOrEmpty(Session["TaiKhoan"].ToString()))
            {
                KHACHHANG kh = (KHACHHANG)Session["TaiKhoan"];
                var lichSuDonHang = db.DONHANGs
                    .Where(dh => dh.MaKH == kh.MaKH)
                    .OrderByDescending(dh => dh.NgayDatHang)
                    .ToList();

                return View(lichSuDonHang);
            }
            else
            {
                // Handle the case when the user is not logged in
                return RedirectToAction("DangNhap", "User");
            }
        }

        [HttpPost]
        public ActionResult HuyDon(int madh)
        {
            if (Session["TaiKhoan"] is KHACHHANG kh)
            {
                var dhToDelete = db.DONHANGs.SingleOrDefault(sp => sp.MaDH == madh);

                if (dhToDelete != null)
                {
                    // Kiểm tra xem đơn hàng đã được thanh toán hay chưa
                    if (dhToDelete.ThanhToan == true || dhToDelete.TrangThai == "Hủy thanh toán")
                    {
                        // Xóa các hàng trong bảng CHITIETDATHANG theo MaDH
                        var chiTietDatHangList = db.CHITIETDATHANGs.Where(ct => ct.MaDH == madh).ToList();
                        db.CHITIETDATHANGs.DeleteAllOnSubmit(chiTietDatHangList);

                        // Xóa đơn hàng
                        db.DONHANGs.DeleteOnSubmit(dhToDelete);

                        db.SubmitChanges();

                        TempData["SuccessMessage"] = "Xóa thông tin đơn hàng thành công";

                        return RedirectToAction("LichSuDonHang");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Đơn hàng chưa được thanh toán!";
                        return View("LichSuDonHang", db.DONHANGs.Where(dh => dh.MaKH == kh.MaKH).OrderByDescending(a => a.NgayDatHang).ToList());
                    }
                }
                else
                {
                    return RedirectToAction("LichSuDonHang");
                }
            }

            return RedirectToAction("DangNhap", "User");
        }

    }

}