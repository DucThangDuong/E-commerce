# Thiết kế luồng xử lý: Thanh toán sai số tiền qua IPN (VNPay)

## 1. Vấn đề
Khi cổng thanh toán (VNPay) gọi Webhook (IPN) về hệ thống, có thể xảy ra trường hợp số tiền báo về (`vnp_Amount`) không khớp với tổng tiền đơn hàng (`TotalAmount`) lưu trong Database. Điều này có thể do lỗi hệ thống, hoặc do có sự cố từ phía cổng thanh toán.

## 2. Luồng xử lý đề xuất (Workflow)
1. **Kiểm tra (Validation):** 
   - Endpoint `PaymentIpnEndpoint` nhận IPN, verify chữ ký.
   - Gọi `ProcessIpnCommand` xuống Handler.
   - Hệ thống tìm `Order` bằng `OrderId`. So sánh `Order.TotalAmount` và `request.Amount`.
2. **Cập nhật Database:**
   - Nếu `TotalAmount != Amount`: 
     - Đánh dấu trạng thái đơn hàng (`Order.Status`) thành `"Failed"`.
     - Đánh dấu trạng thái thanh toán (`Payment.PaymentStatus`) thành `"Fail"`.
     - Lưu lại mã giao dịch `ProviderTransactionId` để đối soát sau này.
     - Lưu xuống DB (`SaveChangesAsync`).
3. **Gửi thông báo qua Message Queue (RabbitMQ):**
   - Lấy `Email` của khách hàng dựa trên `Order.CustomerId`.
   - Publish một event `SendMail` (nằm trong `RabbitMQDTOs.cs`) vào RabbitMQ thông qua `IPublishEndpoint` của MassTransit.
   - Nội dung Email thông báo rõ: *"Giao dịch của đơn hàng #1234 bị ghi nhận sai số tiền (Số tiền thanh toán: X, Số tiền đơn: Y). Đơn hàng đã bị hủy bỏ, vui lòng liên hệ với bộ phận CSKH để được đối soát và hỗ trợ."*
4. **Phản hồi cổng thanh toán (VNPay):**
   - Trả về JSON Response với mã phản hồi `{"RspCode": "04", "Message": "Invalid amount"}` để cổng thanh toán (VNPay) hiểu rằng ứng dụng đã bắt được lỗi này (tiền không khớp) và không gọi lại IPN nữa.

## 3. Các bước cập nhật mã nguồn thực tế
- Tại file `Application/Features/Order/Commands/UpdateHasPaymentHandler.cs`:
  - Khởi tạo tiêm (Inject) thêm `IPublishEndpoint` (MassTransit) và `IAppReadDbContext`.
  - Triển khai logic cập nhật trạng thái đơn hàng (`Status = "Failed"`, `PaymentStatus = "Fail"`).
  - Trích xuất Email bằng LINQ từ bảng Customers.
  - Gọi `await _publishEndpoint.Publish(new SendMail(...))` trước khi return `Result.Failure`.