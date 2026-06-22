# E-commerce API

Nếu bạn đang tìm kiếm một hệ thống API E-commerce mạnh mẽ, hiệu suất cao được xây dựng trên nền tảng .NET 8. Hệ thống được thiết kế để xử lý lượng truy cập lớn với các luồng đặt hàng, thanh toán, quản lý tồn kho và hệ thống giám sát (observability) chuyên sâu.

---

## Live Demo

- **Trải nghiệm Frontend:** [https://e-commerce-frontend-umber-eight.vercel.app](https://e-commerce-frontend-umber-eight.vercel.app)
- **Tài liệu API Backend (Swagger):** [https://shrivel-spool-immovably.ngrok-free.dev/swagger](https://shrivel-spool-immovably.ngrok-free.dev/swagger) 

---

## Công nghệ và kiến trúc

### Công nghệ cốt lõi
* **Framework:** .NET 8.0 SDK / ASP.NET Core
* **API Routing:** [FastEndpoints](https://fast-endpoints.com/) (Thay thế cho Controller truyền thống, tối ưu hiệu năng)
* **Design Pattern:** CQRS với thư viện [MediatR](https://github.com/jbogard/MediatR)
* **ORM:** Entity Framework Core (SQL Server)
* **Caching:** Redis
* **Message Broker:** RabbitMQ với [MassTransit](https://masstransit.io/) 
* **Real-time:** SignalR
* **Logging & Giám sát:** Serilog (Structured Logging), Seq (Log UI), Data Masking, Correlation Id
* **Bảo mật & Validation:** JWT Authentication, FluentValidation, HtmlSanitizer, OAuth2 (Google)
* **Lưu trữ:** Azure Blob Storage
* **Thanh toán:** Tích hợp VNPAY

### Kiến trúc dự án
Dự án được cấu trúc theo nguyên lý Clean Architecture, đảm bảo tính độc lập và dễ mở rộng:

```text
E-commerce/
├── API/              # Presentation Layer: FastEndpoints, Middleware, JWT auth, Logging (Serilog)
├── Application/      # Business Layer: CQRS Handlers, Behaviors (MediatR), DTOs, Consumers
├── Domain/           # Core Layer: Entities, Enums, Constants
├── Infrastructure/   # Data Access Layer: EF Core DbContext, Repositories, VnPay, Azure Storage
├── UnitTest/         # Unit Tests 
└── docker-compose.yml # Container Orchestration (API, Redis, RabbitMQ, Seq, Ngrok)
```

---

## Yêu cầu hệ thống

Trước khi chạy dự án, hãy đảm bảo máy bạn đã cài đặt:
1. **[.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)**
2. **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** (Bắt buộc để chạy nhanh Redis, RabbitMQ và Seq)
3. **SQL Server** (Local hoặc Azure SQL)
4. Tương thích IDE: Visual Studio 2022 hoặc VS Code.

---

## Hướng dẫn chạy dự án

### 1. Clone Source Code
```bash
git clone https://github.com/DucThangDuong/E-commerce.git
cd E-commerce
```

### 2. Thiết lập cấu hình (Configuration)
Bạn cần cung cấp các khóa cấu hình trong `API/appsettings.Development.json` (hoặc thông qua `.env` nếu chạy Docker):

```json
{
  "SecretKey": "your_jwt_secret_key_here",
  "ConnectionStrings": {
    "Ecommerce": "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;",
    "AZURE_SQL_CONNECTIONSTRING": "your_azure_sql_connection_string"
  },
  "Authentication": {
    "Google": {
      "ClientId": "your_google_client_id",
      "ClientSecret": "your_google_client_secret"
    }
  },
  "VnPayConfig": {
    "TmnCode": "your_vnpay_tmn_code",
    "HashSecret": "your_vnpay_hash_secret",
    "ReturnUrl": "https://localhost:7027/order/payment-callback",
    "IpnUrl": "https://your-ngrok.app/order/payment-ipn"
  }
}
```
*(Cấu hình `Serilog` đã được thiết lập sẵn, mặc định trỏ về `http://localhost:5341`)*

### 3. Khởi tạo Database (Database First)
1. Tạo một database mới (ví dụ: `EcommerceDB`) trên SQL Server.
2. Mở file `database.sql` và chạy toàn bộ script để khởi tạo cấu trúc bảng và dữ liệu mẫu.
3. Cập nhật `ConnectionStrings:Ecommerce` trỏ tới database này.

### 4. Chạy các dịch vụ phụ trợ bằng Docker
Để khởi động Redis, RabbitMQ và máy chủ Seq (UI xem log):
```bash
docker-compose up -d redis rabbitmq seq
```
*(Mở trình duyệt truy cập `http://localhost:5341` để theo dõi Log thời gian thực)*

### 5. Build và Chạy Ứng dụng

#### Cách 1: Chạy qua IDE (Local Development)
Sau khi các dịch vụ phụ trợ đã lên, bạn khởi động API:
```bash
dotnet run --project API/API.csproj
```
API sẽ chạy tại `http://localhost:5103` hoặc `https://localhost:7027`. Truy cập `/swagger` để xem tài liệu API.

#### Cách 2: Chạy toàn bộ qua Docker Compose
Nếu muốn tự động hóa, chạy cả API chung với các service khác:
1. Đổi tên file `.env.example` (nếu có) thành `.env` và điền các secret keys.
2. Chạy lệnh:
```bash
docker-compose up -d --build
```

---

## Các tính năng kỹ thuật nổi bật
* **Atomic Stock Deduction:** Ứng dụng xử lý giữ chỗ đơn hàng (Reservation) bằng cách trừ số lượng qua lệnh `DECRBY` nguyên tử trên Redis, triệt tiêu hoàn toàn lỗi bán lố hàng (Overselling).
* **Order Timeout Workflow:** Sử dụng MassTransit & RabbitMQ Delayed Exchange để giới hạn thời gian thanh toán (15 phút). Nếu người dùng không thanh toán, Consumer tự động kích hoạt quá trình hủy đơn và hoàn trả (Refund) số lượng lên Redis.
* **Idempotency Payment:** Kịch bản thanh toán được khóa bởi `Idempotency-Key`, kết hợp Redis đảm bảo giao dịch không bị lặp lại trong trường hợp lỗi đường truyền.
* **Hệ thống Ghi Log thông minh (Serilog + Seq):**
  * **Data Masking:** Tự động che giấu (`***MASKED***`) các dữ liệu nhạy cảm (Password, Token, Credit Card) trước khi ghi xuống đĩa hoặc chuyển lên Seq.
  * **Log Context Enrichment:** Mọi dòng log đều được nhúng tự động mã `CorrelationId`, `UserId` và `IPAddress` để truy vết hành trình của 1 người dùng từ đầu đến cuối một cách dễ dàng.
  * **MediatR Pipeline Logging:** Tự động bắt Payload của mọi API, đo lường thời gian chạy (Performance) và đưa ra cảnh báo `Long Running Request` đối với các thao tác chậm.
