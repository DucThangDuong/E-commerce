# Báo cáo: Ứng dụng của Redis trong dự án E-Commerce

Tài liệu này liệt kê toàn bộ các file, chức năng có sử dụng hệ thống bộ nhớ đệm Redis, mô tả chi tiết mục đích, loại dữ liệu lưu trữ và định dạng dữ liệu trong hệ thống.

---

## 1. Quản lý Phiên Đăng Nhập (Refresh Token)
**Mô tả:** 
- **Mục đích:** Lưu trữ Refresh Token của người dùng sau khi họ đăng nhập thành công. Giúp hệ thống dễ dàng kiểm tra tính hợp lệ khi người dùng xin cấp lại Access Token mới, hoặc cưỡng chế đăng xuất (Revoke) từ xa.
- **Tên Key (Format):** `RefreshToken:{CustomerId}` 
- **Dữ liệu lưu trữ (Value):** `String` (Chuỗi mã Token).
- **Thời gian sống (TTL):** 7 ngày.

---

## 2. Đưa Token vào Danh sách đen (Blacklist Access Token)
**Mô tả:**
- **Mục đích:** Khi người dùng đăng xuất (hoặc bị thu hồi quyền), Access Token hiện tại (dù chưa hết hạn) sẽ bị ném vào danh sách đen. Middleware sẽ chặn mọi request dùng token này.
- **Tên Key (Format):** `Blacklist:{AccessToken}` 
- **Dữ liệu lưu trữ (Value):** `String` (Lưu cứng chuỗi `"banned"`).
- **Thời gian sống (TTL):** 15 phút (Bằng đúng với tuổi thọ của Access Token để tối ưu bộ nhớ).

---

## 3. Chống Trùng Lặp Thanh Toán (Idempotency Key)
**Mô tả:**
- **Mục đích:** Đảm bảo khi Client vô tình gửi 2 request tạo đơn hàng/thanh toán cùng lúc, hệ thống chỉ xử lý 1 lần duy nhất (ngăn chặn tình trạng trừ tiền 2 lần).
- **Tên Key (Format):** `Idempotency:Payment:{IdempotencyKey}`
- **Dữ liệu lưu trữ (Value):** `String`. 
  - Đang xử lý: Lưu chuỗi `"PROCESSING"`.
  - Đã xử lý xong: Lưu chuỗi kết quả (VD: `"Payment created successfully..."` hoặc URL VnPay dạng JSON string).
- **Thời gian sống (TTL):** 2 phút trong lúc đang lock, và gia hạn lên 24 giờ sau khi sinh URL thanh toán.

---

## 4. Tối ưu Đọc Dữ Liệu Sản Phẩm 
**Mô tả:**
- **Mục đích:** Giảm tải truy vấn vào SQL Server. Mỗi khi người dùng xem chi tiết sản phẩm, dữ liệu sẽ được đọc từ Redis.
- **Tên Key (Format):** `Product:Detail:{ProductId}`
- **Dữ liệu lưu trữ (Value):** `JSON String` (Serialize từ object `ResProductDto`).
- **Thời gian sống (TTL):** 10 phút.

---

## 5. Giữ Chỗ Đơn Hàng & Đồng Bộ Kho Hàng (Inventory Reservation)
**Mô tả:**
Hệ thống sử dụng 2 loại Key để giải quyết bài toán trừ kho:

**A. Key Quản lý Tồn kho Tạm thời (Stock)**
- **Mục đích:** Lưu số lượng tồn kho khả dụng để trừ ngay lập tức khi khách bấm Checkout (tránh bán lố).
- **Tên Key (Format):** `Color:Stock:{ColorId}` (Lưu ý: Trong file consumer đang dùng nhầm `Product:Stock:` cần đồng bộ lại).
- **Dữ liệu lưu trữ (Value):** `Integer` (Số lượng tồn kho, dùng `StringDecrementAsync`/`StringIncrementAsync` để tính toán nguyên tử).
- **Thời gian sống (TTL):** 1 ngày (Kéo dài mỗi lần có người mua).

**B. Key Thông tin Đơn hàng Giữ chỗ (Reservation Data)**
- **Mục đích:** Đóng gói thông tin giỏ hàng khách muốn mua để chuyển sang bước Thanh toán.
- **Tên Key (Format):** `Order:Reservation:{ReservationId}`
- **Dữ liệu lưu trữ (Value):** `JSON String` chứa `CustomerId` và `Items` (Mã màu + số lượng). VD: `{"CustomerId":1,"Items":{"10":2}}`.
- **Thời gian sống (TTL):** 15 phút (Nếu sau 15 phút không thanh toán, Consumer sẽ đọc và hoàn lại Stock).
