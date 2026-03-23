IF DB_ID('EcommerceOrderSystem') IS NOT NULL
BEGIN
    ALTER DATABASE EcommerceOrderSystem SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE EcommerceOrderSystem;
END
GO
CREATE DATABASE EcommerceOrderSystem;
GO
USE EcommerceOrderSystem;
GO

-- ==========================================
-- 1. Bảng Khách hàng (Customers)
-- ==========================================
CREATE TABLE Customers (
    customer_id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    passwordHash VARCHAR(255) NOT NULL,
    phone_number VARCHAR(20),
    address NVARCHAR(500),

    createdAt DATETIME NOT NULL DEFAULT GETDATE(),
    refreshToken VARCHAR(256),
    refreshTokenExpiryTime DATETIME,

    role VARCHAR(50) DEFAULT 'User' CHECK(role IN ('User','Admin')),
    loginProvider VARCHAR(20) NULL Check(LoginProvider In('Custom','Google')),
    googleId VARCHAR(255),
    customAvatar VARCHAR(255) DEFAULT 'default-avatar.jpg',
    googleAvatar VARCHAR(255),
    isActive BIT DEFAULT 1,
);

-- ==========================================
-- 2. Bảng Danh mục (Categories)
-- ==========================================
CREATE TABLE Categories (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(500),
    picture VARCHAR(500) 
);

CREATE TABLE Brands (
    brand_id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL UNIQUE, -- Tên hãng xe (Toyota, Honda...)
    logo_url VARCHAR(500),              -- Link ảnh logo của hãng
    description NVARCHAR(MAX)
);
-- ==========================================
-- 3. Bảng Sản phẩm (Products)
-- ==========================================
CREATE TABLE Products (
    product_id INT IDENTITY(1,1) PRIMARY KEY,
    category_id INT NOT NULL,
    brand_id INT,
    name NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
    base_price DECIMAL(18, 2) NOT NULL,
    FOREIGN KEY (category_id) REFERENCES Categories(category_id),
    FOREIGN KEY (brand_id) REFERENCES Brands(brand_id) 
);

-- ==========================================
-- 4. Bảng Tồn kho (Inventory)
-- ==========================================
CREATE TABLE Inventory (
    inventory_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL UNIQUE, 
    stock_quantity INT NOT NULL DEFAULT 0,
    reserved_quantity INT NOT NULL DEFAULT 0,
    last_updated DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (product_id) REFERENCES Products(product_id)
);

-- ==========================================
-- 5. Bảng Đơn hàng (Orders)
-- ==========================================
CREATE TABLE Orders (
    order_id INT IDENTITY(1,1) PRIMARY KEY,
    customer_id INT NOT NULL,
    order_date DATETIME DEFAULT GETDATE(),
    total_amount DECIMAL(18, 2) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (customer_id) REFERENCES Customers(customer_id)
);

-- ==========================================
-- 6. Bảng Chi tiết Đơn hàng (OrderItems)
-- ==========================================
CREATE TABLE OrderItems (
    order_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    unit_price_at_purchase DECIMAL(18, 2) NOT NULL,
    PRIMARY KEY (order_id, product_id), -- Khóa chính kết hợp (Composite Key)
    FOREIGN KEY (order_id) REFERENCES Orders(order_id),
    FOREIGN KEY (product_id) REFERENCES Products(product_id)
);

-- ==========================================
-- 7. Bảng Thanh toán (Payments)
-- ==========================================
CREATE TABLE Payments (
    payment_id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL UNIQUE, -- UNIQUE để đảm bảo quan hệ 1-1 với Order
    amount DECIMAL(18, 2) NOT NULL,
    provider VARCHAR(50) NOT NULL, -- VNPay, MoMo, Stripe, COD...
    payment_status VARCHAR(50) NOT NULL DEFAULT 'Unpaid', -- Paid, Unpaid, Failed
    FOREIGN KEY (order_id) REFERENCES Orders(order_id)
);

-- ==========================================
-- 8. Bảng Giỏ hàng (Cart)
-- ==========================================
CREATE TABLE Cart (
    cart_id INT IDENTITY(1,1) PRIMARY KEY,
    customer_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    FOREIGN KEY (customer_id) REFERENCES Customers(customer_id),
    FOREIGN KEY (product_id) REFERENCES Products(product_id)
);
GO
CREATE TABLE ProductImages (
    image_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL,
    image_url VARCHAR(500) NOT NULL,    -- Đường dẫn lưu file (VD: URL từ MinIO)
    is_primary BIT DEFAULT 0,           -- 1 = Ảnh bìa (Thumbnail chính), 0 = Ảnh phụ
    display_order INT DEFAULT 0,        -- Dùng để sắp xếp thứ tự ảnh hiển thị trên giao diện
    uploaded_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (product_id) REFERENCES Products(product_id) ON DELETE CASCADE
);
GO
CREATE TABLE FeaturedProducts (
    featured_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL UNIQUE,        -- UNIQUE để 1 sản phẩm không bị lặp lại 2 lần trên trang chủ
    display_order INT DEFAULT 0,           -- Thứ tự ưu tiên hiển thị (số nhỏ xếp trước)
    start_date DATETIME DEFAULT GETDATE(), -- Ngày bắt đầu đưa lên trang chủ
    end_date DATETIME NULL,                -- Ngày kết thúc (NULL nghĩa là hiển thị vô thời hạn)
    created_at DATETIME DEFAULT GETDATE(),
    -- Xóa sản phẩm thì tự động xóa khỏi danh sách nổi bật
    FOREIGN KEY (product_id) REFERENCES Products(product_id) ON DELETE CASCADE
);

GO
-- 1. Tạo bảng InboxState (Dùng cho Consumer để chống trùng lặp)
CREATE TABLE [InboxState] (
    [Id] bigint NOT NULL IDENTITY,
    [MessageId] uniqueidentifier NOT NULL,
    [ConsumerId] uniqueidentifier NOT NULL,
    [LockId] uniqueidentifier NOT NULL,
    [RowVersion] rowversion NULL,
    [Received] datetime2 NOT NULL,
    [ReceiveCount] int NOT NULL,
    [ExpirationTime] datetime2 NULL,
    [Consumed] datetime2 NULL,
    [Delivered] datetime2 NULL,
    [LastSequenceNumber] bigint NULL,
    CONSTRAINT [PK_InboxState] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_InboxState_MessageId_ConsumerId] UNIQUE ([MessageId], [ConsumerId])
);
GO


-- 2. Tạo bảng OutboxMessage (Dùng để lưu event chuẩn bị gửi)
CREATE TABLE [OutboxMessage] (
    [SequenceNumber] bigint NOT NULL IDENTITY,
    [EnqueueTime] datetime2 NULL,
    [SentTime] datetime2 NOT NULL,
    [Headers] nvarchar(max) NULL,
    [Properties] nvarchar(max) NULL,
    [InboxMessageId] uniqueidentifier NULL,
    [InboxConsumerId] uniqueidentifier NULL,
    [OutboxId] uniqueidentifier NULL,
    [MessageId] uniqueidentifier NOT NULL,
    [ContentType] nvarchar(256) NOT NULL,
    [MessageType] nvarchar(max) NOT NULL,
    [Body] nvarchar(max) NOT NULL,
    [ConversationId] uniqueidentifier NULL,
    [CorrelationId] uniqueidentifier NULL,
    [InitiatorId] uniqueidentifier NULL,
    [RequestId] uniqueidentifier NULL,
    [SourceAddress] nvarchar(256) NULL,
    [DestinationAddress] nvarchar(256) NULL,
    [ResponseAddress] nvarchar(256) NULL,
    [FaultAddress] nvarchar(256) NULL,
    [ExpirationTime] datetime2 NULL,
    CONSTRAINT [PK_OutboxMessage] PRIMARY KEY ([SequenceNumber])
);
GO

-- 3. Tạo bảng OutboxState (Dùng để MassTransit quản lý trạng thái Worker)
CREATE TABLE [OutboxState] (
    [OutboxId] uniqueidentifier NOT NULL,
    [LockId] uniqueidentifier NOT NULL,
    [RowVersion] rowversion NULL,
    [Created] datetime2 NOT NULL,
    [Delivered] datetime2 NULL,
    [LastSequenceNumber] bigint NULL,
    CONSTRAINT [PK_OutboxState] PRIMARY KEY ([OutboxId])
);
GO

-- 4. Tạo các Index để truy vấn nhanh (Bắt buộc để Worker chạy mượt)
CREATE INDEX [IX_InboxState_Delivered] ON [InboxState] ([Delivered]);
CREATE INDEX [IX_OutboxMessage_EnqueueTime] ON [OutboxMessage] ([EnqueueTime]);
CREATE INDEX [IX_OutboxMessage_ExpirationTime] ON [OutboxMessage] ([ExpirationTime]);
CREATE INDEX [IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber] ON [OutboxMessage] ([InboxMessageId], [InboxConsumerId], [SequenceNumber]);
CREATE INDEX [IX_OutboxMessage_OutboxId_SequenceNumber] ON [OutboxMessage] ([OutboxId], [SequenceNumber]);
CREATE INDEX [IX_OutboxState_Created] ON [OutboxState] ([Created]);
GO

INSERT INTO Categories (name, description, picture) VALUES
(N'Xe số', N'Xe số phổ thông tiết kiệm xăng', 'img/xe-so.jpg'),
(N'Xe tay ga', N'Xe tay ga tiện lợi đô thị', 'img/xe-tay-ga.jpg'),
(N'Xe côn tay', N'Xe thể thao côn tay', 'img/xe-con.jpg'),
(N'Xe PKL', N'Phân khối lớn', 'img/pkl.jpg'),
(N'Xe điện', N'Thân thiện môi trường', 'img/dien.jpg'),
(N'Xe cổ điển', N'Phong cách vintage', 'img/classic.jpg'),
(N'Xe thể thao', N'Thiết kế mạnh mẽ', 'img/sport.jpg'),
(N'Xe touring', N'Đi đường dài', 'img/touring.jpg'),
(N'Xe mini', N'Nhỏ gọn', 'img/mini.jpg'),
(N'Xe nhập khẩu', N'Hàng cao cấp', 'img/import.jpg');

INSERT INTO Brands (name, logo_url, description) VALUES
(N'Honda', 'logo/honda.png', N'Hãng xe Nhật Bản'),
(N'Yamaha', 'logo/yamaha.png', N'Hãng xe thể thao'),
(N'Suzuki', 'logo/suzuki.png', N'Xe bền bỉ'),
(N'SYM', 'logo/sym.png', N'Giá rẻ'),
(N'Piaggio', 'logo/piaggio.png', N'Phong cách Ý'),
(N'Kawasaki', 'logo/kawasaki.png', N'PKL mạnh mẽ'),
(N'Ducati', 'logo/ducati.png', N'Xe Ý cao cấp'),
(N'BMW', 'logo/bmw.png', N'Xe Đức'),
(N'KTM', 'logo/ktm.png', N'Thể thao'),
(N'VinFast', 'logo/vinfast.png', N'Xe Việt Nam');

INSERT INTO Products (category_id, brand_id, name, description, base_price) VALUES
(1,1,N'Honda Wave Alpha',N'Xe số quốc dân',18000000),
(1,2,N'Yamaha Sirius',N'Xe số bền',19000000),
(2,1,N'Honda Vision',N'Xe ga phổ biến',31000000),
(2,2,N'Yamaha Janus',N'Xe ga nhẹ',29000000),
(2,1,N'Honda Lead',N'Cốp rộng',39000000),

(3,2,N'Yamaha Exciter 155',N'Côn tay mạnh',47000000),
(3,1,N'Honda Winner X',N'Côn tay thể thao',46000000),
(2,1,N'Honda Air Blade',N'Thiết kế đẹp',56000000),
(2,2,N'Yamaha NVX',N'Thể thao',54000000),
(2,5,N'Piaggio Liberty',N'Cao cấp',58000000),

(2,5,N'Vespa Sprint',N'Phong cách Ý',75000000),
(2,1,N'Honda SH Mode',N'Sang trọng',58000000),
(2,1,N'Honda SH160',N'Cao cấp',92000000),
(1,3,N'Suzuki Viva',N'Tiết kiệm xăng',21000000),
(1,4,N'SYM Galaxy',N'Giá rẻ',17000000),

(4,6,N'Kawasaki Z1000',N'PKL mạnh',400000000),
(3,2,N'Yamaha R15',N'Sportbike',70000000),
(3,1,N'Honda CBR150R',N'Thể thao',72000000),
(3,3,N'Suzuki Raider',N'Tốc độ cao',50000000),
(1,1,N'Honda Future',N'Cao cấp',32000000),

(1,2,N'Yamaha Jupiter',N'Mạnh mẽ',30000000),
(2,4,N'SYM Attila',N'Tiện lợi',35000000),
(2,4,N'Kymco Like',N'Cổ điển',36000000),
(2,5,N'Peugeot Django',N'Xe Pháp',68000000),
(4,7,N'Ducati Monster',N'PKL Ý',410000000),

(4,8,N'BMW G310R',N'Xe Đức',150000000),
(4,9,N'KTM Duke 390',N'Thể thao',160000000),
(3,2,N'Yamaha MT15',N'Naked bike',78000000),
(2,1,N'Honda PCX',N'Ga cao cấp',88000000),
(2,2,N'Yamaha Grande',N'Tiết kiệm',46000000),

(2,3,N'Suzuki Burgman',N'Touring',49000000),
(1,1,N'Honda Blade',N'Giá rẻ',18500000),
(1,4,N'SYM Elegant',N'50cc',16000000),
(5,10,N'VinFast Evo200',N'Xe điện',22000000),
(5,10,N'VinFast Klara',N'Cao cấp',39000000),

(5,10,N'VinFast Feliz',N'Tiết kiệm',30000000),
(4,6,N'Kawasaki Ninja 400',N'Sportbike',180000000),
(4,7,N'Ducati Panigale',N'Siêu xe',700000000),
(4,8,N'BMW S1000RR',N'Siêu moto',800000000),
(4,9,N'KTM RC390',N'Sport',170000000),

(2,5,N'Vespa GTS',N'Cao cấp',95000000),
(2,1,N'Honda Scoopy',N'Nhập khẩu',38000000),
(2,2,N'Yamaha Latte',N'Cho nữ',38000000),
(1,1,N'Honda Dream',N'Huyền thoại',25000000),
(3,9,N'KTM Duke 200',N'Thể thao',120000000),

(3,8,N'BMW G310GS',N'Adventure',170000000),
(4,6,N'Kawasaki Versys',N'Touring',300000000),
(2,5,N'Lambretta V200',N'Cổ điển',86000000),
(2,1,N'Honda ADV160',N'Adventure',90000000),
(3,2,N'Yamaha XSR155',N'Neo classic',75000000);