# E-commerce API

Nếu bạn đang tìm kiếm một hệ thống API E-commerce mạnh mẽ, hiệu suất cao được xây dựng trên nền tảng .NET 8. Hệ thống được thiết kế để xử lý lượng truy cập lớn với các luồng đặt hàng, thanh toán, quản lý tồn kho.

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
* **Bảo mật & Validation:** JWT Authentication, FluentValidation, HtmlSanitizer, OAuth2
* **Lưu trữ:** Azure 

### Kiến trúc dự án
Dự án được cấu trúc theo nguyên lý Clean Architecture, đảm bảo tính độc lập và dễ mở rộng:

```text
E-commerce/
├── API/              # Presentation Layer: FastEndpoints, Middleware, JWT auth, DI configuration
├── Application/      # Business Logic Layer: CQRS Handlers (MediatR), DTOs, Consumers, Interfaces
├── Domain/           # Core Layer: Entities, Enums, Constants
├── Infrastructure/   # Data Access Layer: EF Core DbContext, Repositories, VnPay, Azure Storage, Email Services
├── UnitTest/         # Unit Tests 
└── docker-compose.yml
```

---

## Yêu cầu hệ thống

Trước khi chạy dự án, hãy đảm bảo máy bạn đã cài đặt:
1. **[.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)**
2. **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** (Bắt buộc để chạy nhanh Redis và RabbitMQ)
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
Dự án sử dụng file `appsettings.json`. 
Bạn cần cung cấp các khóa cấu hình sau trong `API/appsettings.Development.json` (hoặc cấu hình thông qua `.env` nếu chạy Docker):

```json
{
  "SecretKey": "your_jwt_secret_key_here",
  "ConnectionStrings": {
    "Ecommerce": "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;",
    "AzureStorageAccount": "your_azure_blob_connection_string",
    "AZURE_SQL_CONNECTIONSTRING": "your_azure_sql_connection_string"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest",
    "Port": "5672"
  },
  "Authentication": {
    "Google": {
      "ClientId": "your_google_client_id",
      "ClientSecret": "your_google_client_secret"
    }
  },
  "MailSettings": {
    "UserName": "your_email@gmail.com",
    "Password": "your_app_password"
  },
  "VnPayConfig": {
    "TmnCode": "your_vnpay_tmn_code",
    "HashSecret": "your_vnpay_hash_secret",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "http://localhost:your_port/api/payment/callback",
    "IpnUrl": "http://localhost:your_port/api/payment/ipn",
    "Version": "2.1.0"
  },
  "RedisCache": "localhost:6379"
}
```

### 3. Khởi tạo Database (Database First)
Dự án này sử dụng phương pháp Database First. Bạn cần tạo database và các bảng trước khi chạy ứng dụng.
1. Mở SQL Server Management Studio (SSMS) hoặc công cụ quản lý cơ sở dữ liệu tương tự.
2. Tạo một database mới (ví dụ: `EcommerceDB`).
3. Mở file `database.sql` nằm ở thư mục gốc của dự án và chạy (Execute) toàn bộ script trong file đó trên database vừa tạo để khởi tạo cấu trúc bảng và dữ liệu mẫu.
4. Cập nhật chuỗi kết nối (`ConnectionStrings:Ecommerce`) trong file `appsettings.json` trỏ tới database này.

### 4. Chạy các dịch vụ ngầm (Redis & RabbitMQ) bằng Docker
Để khởi động Redis và RabbitMQ mà không cần cài đặt phức tạp, hãy sử dụng `docker-compose.yml` đi kèm:

```bash
docker-compose up -d redis rabbitmq
```

### 5. Build và Chạy Ứng dụng

#### Cách 1: Chạy trực tiếp qua CLI
Sau khi đã thiết lập xong Database và các dịch vụ ngầm, khởi động API:

```bash
# Chạy dự án
dotnet run --project API/API.csproj
```
API sẽ khởi chạy tại `http://localhost:5103` hoặc `https://localhost:7027`. Bạn có thể truy cập `/swagger` để xem tài liệu API.

#### Cách 2: Chạy toàn bộ qua Docker Compose
Nếu bạn muốn chạy ứng dụng API và mọi Redis/Message Broker hoàn toàn trong Docker:

1. Đổi tên file `.env.example` (nếu có) thành `.env` và điền các secret keys thực tế.
2. Chạy lệnh:
```bash
docker-compose up -d
```
Docker sẽ tải Image, link các containers với nhau (`ecommerce_api`, `ecommerce_redis`, `ecommerce_rabbitmq`) và expose cổng `8080`.

---

## Các tính năng nổi bật về luồng dữ liệu
* **Atomic Stock Deduction:** Ứng dụng xử lý giữ chỗ đơn hàng (Reservation) bằng cách trừ số lượng trực tiếp qua lệnh `DECRBY` nguyên tử trên Redis, triệt tiêu lỗi bán lố hàng (Overselling) trong mô hình concurrent.
* **Order Timeout Workflow:** Sử dụng MassTransit & RabbitMQ Delayed Exchange để giới hạn thời gian thanh toán (15 phút). Nếu người dùng không thanh toán, Consumer tự động kích hoạt quá trình hủy đơn và hoàn trả (Refund) số lượng lên Redis.
* **Idempotency:** Kịch bản tạo Payment được khóa bởi `Idempotency-Key`, đảm bảo giao dịch không bị lặp lại trong trường hợp lỗi đường truyền.
