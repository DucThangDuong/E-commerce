# ĐẶC TẢ LUỒNG XỬ LÝ ĐẶT HÀNG & THANH TOÁN (CHECKOUT WORKFLOW)

## 1. CÔNG NGHỆ VÀ KIẾN TRÚC SỬ DỤNG
* **API Layer:** FastEndpoints.
* **Application Layer:** MediatR (CQRS pattern).
* **Data Access:** EF Core (Repository & Unit of Work pattern).
* **Caching/State:** Redis (MGET, DECRBY, INCRBY).
* **Async Messaging:** RabbitMQ (Delayed Messages / Dead Letter Exchange).

---

## 2. GIAI ĐOẠN 1: KIỂM TRA SỐ LƯỢNG SẢN PHẨM (READ-ONLY)
**Endpoint:** `POST /order`
**Mục đích:** Kiểm tra tính hợp lệ của giỏ hàng và tồn kho trước khi người dùng điền thông tin chi tiết.

### 2.1. Logic Thực thi
1. Nhận <ReqAddNewOrder> từ Frontend.
2. Khởi tạo mảng keys cho Redis: `Color:Stock:{id}`.
3. Thực thi lệnh Redis `MGET` để lấy số lượng tồn kho hiện tại cho tất cả các sản phẩm được yêu cầu.
4. So sánh số lượng yêu cầu (`Quantity`) với số lượng tồn kho trả về từ Redis.
5. **Nhánh A (Không đủ tồn kho):**
   * Ngừng xử lý.
   * Trả về `HTTP 400 Bad Request`.
   * Payload: Danh sách các sản phẩm bị thiếu kèm theo số lượng yêu cầu và số lượng thực tế còn lại.
6. **Nhánh B (Đủ tồn kho):**
   * Truy vấn SQL Server (qua EF Core) để lấy giá gốc (`UnitPrice`) mới nhất của từng sản phẩm.
   * Tính toán tổng tiền tạm tính (`SubTotal`).
   * Trả về `HTTP 200 OK` với thông tin chi tiết các sản phẩm (đã kèm giá gốc chuẩn) và `SubTotal`.

---

## 3. GIAI ĐOẠN 2: THỰC HIỆN TÍNH TOÁN KHI ÁP MÃ GIẢM GIÁ (READ-ONLY)
**Endpoint:** `POST /order/calculate`
**Mục đích:** Tính toán động tổng số tiền khi người dùng thay đổi địa chỉ (phí ship) hoặc áp dụng mã khuyến mãi.

### 3.1. Logic Thực thi
1. Nhận <ReqAddNewOrder>, `ShippingAddressId` (nếu có), và `CouponCode` từ Frontend.
2. Tính toán lại `SubTotal` (tương tự logic Giai đoạn 1).
3. **Kiểm tra tính hợp lệ của Mã giảm giá (Coupon):**
   * Truy vấn SQL Server để kiểm tra `CouponCode` (hạn sử dụng, giới hạn số lần dùng, giá trị đơn hàng tối thiểu).
   * Nếu không hợp lệ: Trả về lỗi `HTTP 400`.
   * Nếu hợp lệ: Tính toán số tiền được giảm (`DiscountAmount`).
4. Tính toán số tiền cuối cùng: `FinalAmount = SubTotal + ShippingFee - DiscountAmount`.
5. Trả về `HTTP 200 OK` với tất cả các thông tin chi tiết đã tính toán. **(Tuyệt đối KHÔNG giữ chỗ/trừ tồn kho ở bước này).**

---

## 4. GIAI ĐOẠN 3: BƯỚC THANH TOÁN & GIỮ CHỖ (TRANSACTIONAL)
**Endpoint:** `POST /order/create-payment`
**Command:** `CreatePaymentCommand` (MediatR)
**Mục đích:** Khóa số lượng sản phẩm, lưu đơn hàng vào Database và tạo thông tin giao dịch.

### 4.1. Logic Thực thi (Quy tắc All-or-Nothing)
1. **Khóa Tồn Kho Cứng (Hard Lock):**
   * Thực thi Redis `MGET` để kiểm tra tồn kho lần cuối.
   * Nếu không đủ: Trả về `HTTP 400`.
   * Nếu đủ: Ngay lập tức thực thi Redis `DECRBY` cho từng sản phẩm để trừ đi số lượng và giữ chỗ.
2. **Chốt Giá Cuối Cùng:** * Chạy lại toàn bộ logic của Giai đoạn 2 ở backend để đảm bảo con số `FinalAmount` là chính xác tuyệt đối, không bị làm giả từ Frontend.
3. **Lưu Database (Unit of Work):**
   * Thêm dữ liệu vào bảng `Orders` (Trạng thái: `Pending` hoặc `Processing_Payment`, `TotalAmount`: `FinalAmount`, Snapshot của `CouponCode`).
   * Thêm dữ liệu vào bảng `OrderItems` (Snapshot của `UnitPrice` và `Quantity`).
   * Commit transaction xuống SQL Server.
4. **Xử lý Phương thức Thanh toán:**
   * **Nếu là COD:** Cập nhật trạng thái đơn thành `Pending`. Trả về `OrderId` và thông báo thành công.
   * **Nếu là VNPAY (QR):** Cập nhật trạng thái đơn thành `Processing_Payment`. Gọi SDK sinh ra URL thanh toán chứa `OrderId` và `FinalAmount`. Trả về `OrderId` + `PaymentUrl`.
5. **Kích hoạt Hẹn giờ Hủy đơn:**
   * Bắn một Event (ví dụ: `OrderTimeoutEvent { OrderId }`) vào RabbitMQ Exchange với cấu hình **Delay 15 phút**.

---

## 5. GIAI ĐOẠN 4: XỬ LÝ CHẠY NGẦM SAU KHI ĐẶT HÀNG (ASYNC BACKGROUND)

### 5.1. Xử lý IPN Webhook (VNPAY Trả kết quả)
**Mục đích:** Nhận kết quả thanh toán từ cổng thanh toán.
**Logic:**
1. Xác thực chữ ký hash của VNPAY.
2. Tìm đơn hàng trong Database theo `vnp_TxnRef`.
3. Kiểm tra nghiêm ngặt: `vnp_Amount` (tiền thực khách đã trả) có khớp với `Order.TotalAmount` không.
   * Nếu sai lệch: Cập nhật trạng thái đơn thành `Payment_Mismatch`. Trả về mã lỗi cho VNPAY (vd: `04`).
4. Nếu hợp lệ và thanh toán thành công: 
   * Đổi trạng thái đơn sang `Paid`.
   * Kích hoạt các luồng phụ: Gửi Email xác nhận, đẩy thông báo SignalR cho Admin.
   * Trả về mã thành công cho VNPAY (`00`).

### 5.2. Hàng đợi Nhả Tồn Kho (RabbitMQ Consumer)
**Trigger:** Consumer nhận được tin nhắn sau khi thời gian delay 15 phút kết thúc.
**Logic:**
1. Kiểm tra trạng thái của `OrderId` trong Database.
2. Nếu đơn hàng đã ở trạng thái `Paid` hoặc `Pending` (COD) -> Đơn hàng thành công, Consumer chỉ cần Acknowledge và bỏ qua tin nhắn.
3. Nếu đơn hàng vẫn kẹt ở trạng thái `Processing_Payment`:
   * Hệ thống xác định khách hàng không thanh toán đúng hạn. Đổi trạng thái đơn sang `Cancelled`.
   * Lấy danh sách `OrderItems` của đơn hàng đó.
   * Thực thi lệnh Redis `INCRBY` để cộng trả lại số lượng tồn kho cho các sản phẩm, sẵn sàng cho người khác mua.
   * Lưu thay đổi vào Database.