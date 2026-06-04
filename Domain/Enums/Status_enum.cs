using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum PaymentStatus
    {
        Fail = -1,           // Thất bại (Lỗi thanh toán)
        Payment_Mismatch = -2, // Sai số tiền thanh toán
        Pending = 0,         // Đang chờ xử lý
        Paid = 1,            // Thành công (Đã thanh toán)
        Unpaid = 2           // Chưa thanh toán (COD)
    }

    public enum OrderStatus
    {
        Cancelled = -1,          // Đã hủy
        Failed = -2,             // Bị lỗi (Vd: Thanh toán thất bại)
        Pending = 0,             // Đang chờ xử lý (COD)
        Processing_Payment = 1,  // Đang chờ thanh toán online (VnPay)
        Confirmed = 2,           // Đã xác nhận / Chuẩn bị hàng
        Shipping = 3,            // Đang giao hàng
        Completed = 4            // Hoàn thành
    }

    public enum Gender
    {
        Female = 0,  // Nữ
        Male = 1,    // Nam
        Other = 2    // Khác
    }

    public enum UserRole
    {
        User = 0,    // Khách hàng
        Admin = 1    // Quản trị viên
    }

    public enum PaymentProvider
    {
        COD = 0,     // Thanh toán khi nhận hàng
        VnPay = 1    // Thanh toán qua cổng VNPay
    }

    public enum DiscountType
    {
        FixedAmount = 0, // Giảm theo số tiền cố định
        Percentage = 1   // Giảm theo phần trăm
    }

    public enum LoginProvider
    {
        Custom = 0,  // Đăng nhập truyền thống (Email/Password)
        Google = 1   // Đăng nhập qua Google
    }
}
