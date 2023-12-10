function KiemTraThongTin(){
    var hoTen=document.getElementById("hoten");
    if (hoTen.value==""){
        alert("Vui lòng nhập Họ tên.");
        hoTen.focus();
        return false;
    }
    var quocGia=document.getElementById("quocgia");
    if (quocGia.value==""){
        alert("Vui lòng nhập quốc gia của bạn.");
        quocGia.focus();
        return false;
    }
    var diaChi=document.getElementById("diachi");
    if (diaChi.value==""){
        alert("Vui lòng nhập vào địa chỉ của bạn.");
        diaChi.focus();
        return false;
    }
    var thanhPho=document.getElementById("thanhpho");
    if (thanhPho.value==""){
        alert("Vui lòng nhập vào thành phố/tỉnh.");
        thanhPho.focus();
        return false;
    }
    var sodienThoai=document.getElementById("sodienthoai");
    if (sodienThoai.value==""){
        alert("Xin mời nhập vào số điện thoại.");
        sodienThoai.focus();
        return false;
    }
    var email=window.document.getElementById("email") ;
    re=/\w+@\w+\.\w+/;
    if (email.value=="") {
        alert("Bạn chưa nhập Email.") ;
        email. focus () ;
        return false;
    }
    else
    if (re. test (email. value) ==false) {
        alert("Email không đúng định dạng!");
        email. focus () ;
        return false;
    }
    var Checkbox=document.getElementById("checkbox");
    if (Checkbox.checked == false){
        alert("Xin vui lòng xác nhận thông tin.");
        Checkbox.focus();
        return false;
    }
    var thanhToan=document.getElementById("thanhtoan");
    if (thanhToan.checked == false){
        alert("Xin vui lòng chọn thanh toán để hoàn thành.");
        thanhToan.focus();
        return false;
    }
    alert("Bạn đã cập nhật thông tin thành công.");
    return true;
}
        
        
        d = new Date();
        thu = d.getDay() ; ngay= d.getDate();
        ngay= ((ngay< 10) ? '0' : '') + ngay;
        thang= d.getMonth()+1;
        thang= ((thang< 10) ? '0' : '') + thang;
        nam= 1900+d.getYear();
        gio = d.getHours();
        gio=((gio<10) ? '0' : '') + gio;
        phut = d.getMinutes();
        phut= ((phut< 10) ? '0' : '') + phut;
        giay = d.getSeconds();
        if (giay < 10) giay='0'+qiay;
        if (thu == 0) thu = " Chủ nhật";
        if (thu == 1) thu = " Thứ hai";
        if (thu == 2) thu = " Thứ ba";
        if (thu == 3) thu = " Thứ tư";
        if (thu == 4) thu = " Thứ năm";
        if (thu == 5) thu = " Thứ sáu";
        if (Lhu == 6) thu = " Thứ bảy";



        
        