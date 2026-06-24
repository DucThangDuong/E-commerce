using Application.DTOs.Services;
using Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string CreatePaymentUrl(int orderId, decimal amount, string ipAddress)
        {
            string vnp_Returnurl = _configuration["VnPayConfig:ReturnUrl"] ?? "";
            string vnp_Url = _configuration["VnPayConfig:BaseUrl"] ?? "";
            string vnp_TmnCode = _configuration["VnPayConfig:TmnCode"] ?? "";
            string vnp_HashSecret = _configuration["VnPayConfig:HashSecret"] ?? "";
            string vnp_IpnUrl = _configuration["VnPayConfig:IpnUrl"] ?? "";

            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
            {
                ipAddress = "127.0.0.1";
            }

            var vnpay = new VnPayLibrary();
            var tick = DateTime.Now.Ticks.ToString();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());
            vnpay.AddRequestData("vnp_BankCode", "");
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", orderId.ToString());
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_TxnRef", orderId.ToString());
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss"));

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return paymentUrl;
        }
        public ResVnPayDTO PaymentCallback(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();

            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            string vnp_HashSecret = _configuration["VnPayConfig:HashSecret"] ?? "";

            string orderId = vnpay.GetResponseData("vnp_TxnRef");
            string vnpayTranId = vnpay.GetResponseData("vnp_TransactionNo");
            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string? vnp_SecureHash = collections["vnp_SecureHash"];

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

            if (checkSignature)
            {
                if (vnp_ResponseCode == "00")
                {
                    return new ResVnPayDTO { Success = true, Message = "Thanh toán thành công!", OrderId = orderId, TransactionId = vnpayTranId };
                }
                return new ResVnPayDTO { Success = false, Message = $"Thanh toán lỗi. Mã lỗi: {vnp_ResponseCode}", OrderId = orderId, TransactionId = vnpayTranId };
            }
            return new ResVnPayDTO { Success = false, Message = "Có lỗi xảy ra trong quá trình xử lý (Sai chữ ký bảo mật)." };
        }

        public bool ValidateSignature(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();

            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }
            var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value.ToString();
            string vnp_HashSecret = _configuration["VnPayConfig:HashSecret"] ?? "";
            return vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
        }
    }
}
