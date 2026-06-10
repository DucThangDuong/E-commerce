-- ====================================================================
-- PHẦN 1: CÁC BẢNG ĐỘC LẬP (Không chứa khóa ngoại)
-- ====================================================================

CREATE TABLE Customers (
    customer_id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    passwordHash VARCHAR(255) NOT NULL,
    phone_number VARCHAR(20),
    address NVARCHAR(500),
    createdAt DATETIME NOT NULL DEFAULT GETDATE(),
    role VARCHAR(20) DEFAULT 'User' CHECK(role IN ('User', 'Admin')),
    loginProvider VARCHAR(20) NULL CHECK(loginProvider IN ('Custom', 'Google')),
    googleId VARCHAR(255),
    customAvatar VARCHAR(255) DEFAULT 'default-avatar.jpg',
    googleAvatar VARCHAR(255),
    isActive BIT DEFAULT 1
);
GO

CREATE TABLE Categories (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(500),
    picture VARCHAR(500) 
);
GO

CREATE TABLE Brands (
    brand_id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL UNIQUE, 
    logo_url VARCHAR(500),              
    description NVARCHAR(MAX)
);
GO

CREATE TABLE Specifications (
    spec_id INT IDENTITY(1,1) PRIMARY KEY,
    spec_name NVARCHAR(255) NOT NULL UNIQUE,
    display_order INT DEFAULT 0
);
GO

CREATE TABLE Promotions (
    promotion_id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255) NOT NULL, 
    discount_type VARCHAR(20) NOT NULL CHECK(discount_type IN ('percentage', 'fixed_amount')), 
    discount_value DECIMAL(15, 2) NOT NULL, 
    start_date DATETIME NOT NULL,
    end_date DATETIME NOT NULL,
    is_active BIT DEFAULT 1,
    created_at DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Coupons (
    coupon_id INT IDENTITY(1,1) PRIMARY KEY,
    code VARCHAR(50) UNIQUE NOT NULL,
    name NVARCHAR(255) NOT NULL,
    discount_type VARCHAR(20) NOT NULL CHECK(discount_type IN ('percentage', 'fixed_amount')), 
    discount_value DECIMAL(15, 2) NOT NULL,
    min_order_value DECIMAL(15, 2) DEFAULT 0,
    max_discount_amount DECIMAL(15, 2) NULL,
    usage_limit INT NULL,
    used_count INT DEFAULT 0,
    usage_limit_per_user INT DEFAULT 1,
    start_date DATETIME NOT NULL,                
    end_date DATETIME NOT NULL,                  
    is_active BIT DEFAULT 1,
    created_at DATETIME DEFAULT GETDATE()
);
GO

-- ====================================================================
-- PHẦN 2: CÁC BẢNG CẤP 1 (Tham chiếu đến bảng độc lập)
-- ====================================================================

CREATE TABLE Products (
    product_id INT IDENTITY(1,1) PRIMARY KEY,
    category_id INT NOT NULL,
    brand_id INT,
    name NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
    base_price DECIMAL(18, 2) NOT NULL,
    FOREIGN KEY (category_id) REFERENCES Categories(category_id) ON DELETE CASCADE,
    FOREIGN KEY (brand_id) REFERENCES Brands(brand_id) ON DELETE CASCADE
);
GO

CREATE TABLE Orders (
    order_id INT IDENTITY(1,1) PRIMARY KEY,
    customer_id INT NOT NULL,
    order_date DATETIME DEFAULT GETDATE(),
    coupon_id INT NULL,
    discount_amount DECIMAL(15, 2) DEFAULT 0,
    total_amount DECIMAL(18, 2) NOT NULL,
    status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (customer_id) REFERENCES Customers(customer_id) ON DELETE CASCADE,
    FOREIGN KEY (coupon_id) REFERENCES Coupons(coupon_id) ON DELETE SET NULL
);
GO

-- ====================================================================
-- PHẦN 3: CÁC BẢNG CẤP 2 (Tham chiếu đến bảng Cấp 1)
-- ====================================================================

CREATE TABLE ProductColors (
    color_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL,
    color_name NVARCHAR(50) NOT NULL,
    price_adjustment DECIMAL(15, 2) DEFAULT 0, 
    FOREIGN KEY (product_id) REFERENCES Products(product_id) ON DELETE CASCADE
);
GO

CREATE TABLE ProductSpecifications (
    product_id INT NOT NULL,
    spec_id INT NOT NULL,
    spec_value NVARCHAR(500) NOT NULL,
    PRIMARY KEY (product_id, spec_id),
    FOREIGN KEY (product_id) REFERENCES Products(product_id) ON DELETE CASCADE,
    FOREIGN KEY (spec_id) REFERENCES Specifications(spec_id) ON DELETE CASCADE
);
GO

CREATE TABLE ProductPromotions (
    product_id INT NOT NULL,
    promotion_id INT NOT NULL,
    PRIMARY KEY (product_id, promotion_id),
    FOREIGN KEY (product_id) REFERENCES Products(product_id) ON DELETE CASCADE,
    FOREIGN KEY (promotion_id) REFERENCES Promotions(promotion_id) ON DELETE CASCADE
);
GO

CREATE TABLE FeaturedProducts (
    featured_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL UNIQUE,        
    display_order INT DEFAULT 0,            
    start_date DATETIME DEFAULT GETDATE(), 
    end_date DATETIME NULL,                
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (product_id) REFERENCES Products(product_id) ON DELETE CASCADE
);
GO

CREATE TABLE OrderShippingDetails (
    order_id INT PRIMARY KEY, 
    recipient_name NVARCHAR(100) NOT NULL,
    recipient_phone VARCHAR(20) NOT NULL,
    street_address NVARCHAR(255) NOT NULL,
    customer_note NVARCHAR(500) NULL,
    FOREIGN KEY (order_id) REFERENCES Orders(order_id) ON DELETE CASCADE 
);
GO

CREATE TABLE Payments (
    payment_id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL UNIQUE, 
    amount DECIMAL(18, 2) NOT NULL,
    provider VARCHAR(50) NOT NULL, 
    payment_status VARCHAR(50) NOT NULL DEFAULT 'Unpaid', 
    provider_transaction_id VARCHAR(50), 
    idempotency_key VARCHAR(50),         
    FOREIGN KEY (order_id) REFERENCES Orders(order_id) ON DELETE CASCADE
);
GO

CREATE TABLE CouponUsages (
    usage_id INT IDENTITY(1,1) PRIMARY KEY,
    coupon_id INT NOT NULL,
    customer_id INT NOT NULL, 
    order_id INT NOT NULL,    
    used_at DATETIME DEFAULT GETDATE(),
    UNIQUE (coupon_id, customer_id, order_id), 
    FOREIGN KEY (coupon_id) REFERENCES Coupons(coupon_id) ON DELETE CASCADE,
    FOREIGN KEY (customer_id) REFERENCES Customers(customer_id), 
    FOREIGN KEY (order_id) REFERENCES Orders(order_id)
);
GO

-- ====================================================================
-- PHẦN 4: CÁC BẢNG CẤP 3 (Tham chiếu đến bảng Cấp 2 và các bảng khác)
-- ====================================================================

CREATE TABLE ProductImages (
    image_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL,
    color_id INT NULL,
    image_url VARCHAR(500) NOT NULL,    
    is_primary BIT DEFAULT 0,            
    display_order INT DEFAULT 0,        
    uploaded_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (product_id) REFERENCES Products(product_id) ON DELETE CASCADE,
    FOREIGN KEY (color_id) REFERENCES ProductColors(color_id) 
);
GO

CREATE TABLE Inventory (
    inventory_id INT IDENTITY(1,1) PRIMARY KEY,
    color_id INT NOT NULL UNIQUE, 
    stock_quantity INT NOT NULL DEFAULT 0,
    reserved_quantity INT NOT NULL DEFAULT 0,
    last_updated DATETIME DEFAULT GETDATE(), 
    FOREIGN KEY (color_id) REFERENCES ProductColors(color_id) ON DELETE CASCADE
);
GO

CREATE TABLE OrderItems (
    order_id INT NOT NULL,
    color_id INT NOT NULL, 
    quantity INT NOT NULL CHECK (quantity > 0),
    unit_price_at_purchase DECIMAL(18, 2) NOT NULL,
    PRIMARY KEY (order_id, color_id), 
    FOREIGN KEY (order_id) REFERENCES Orders(order_id) ON DELETE CASCADE,
    FOREIGN KEY (color_id) REFERENCES ProductColors(color_id)
);
GO

CREATE TABLE Cart (
    cart_id INT IDENTITY(1,1) PRIMARY KEY,
    customer_id INT NOT NULL,
    color_id INT NOT NULL,  
    quantity INT NOT NULL CHECK (quantity > 0),
    UNIQUE (customer_id, color_id), 
    FOREIGN KEY (customer_id) REFERENCES Customers(customer_id) ON DELETE CASCADE,
    FOREIGN KEY (color_id) REFERENCES ProductColors(color_id)
);
GO
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
-- ==========================================
-- 1. CHÈN DỮ LIỆU DANH MỤC (Categories)
-- ==========================================
INSERT INTO Categories (name, description, picture) VALUES
(N'Xe số', N'Dòng xe phổ thông với thiết kế nhỏ gọn, động cơ bền bỉ và khả năng tiết kiệm nhiên liệu vượt trội. Phù hợp cho mọi nhu cầu từ đi học, đi làm đến chuyên chở hàng hóa hàng ngày. Dễ dàng bảo dưỡng với chi phí thấp nhất.', 'img/xe-so.jpg'),
(N'Xe tay ga', N'Mang đến sự tiện lợi tối đa cho việc di chuyển trong đô thị đông đúc. Sở hữu thiết kế thời trang, cốp chứa đồ siêu rộng, thao tác vận hành đơn giản cùng nhiều tiện ích công nghệ hiện đại đi kèm như khóa thông minh, phanh ABS.', 'img/xe-tay-ga.jpg'),
(N'Xe côn tay', N'Dòng xe mang đậm phong cách thể thao, dành cho những ai đam mê tốc độ và muốn làm chủ hoàn toàn sức mạnh động cơ. Thao tác bóp côn gảy số mang lại cảm giác lái phấn khích, khả năng tăng tốc ấn tượng và linh hoạt.', 'img/xe-con.jpg'),
(N'Xe PKL', N'Những cỗ máy sức mạnh mang dung tích xy-lanh từ 175cc trở lên. Đây là biểu tượng của đẳng cấp, tốc độ và sự tự do. Âm thanh ống xả uy lực cùng loạt công nghệ hỗ trợ lái tiên tiến nhất, mang đến trải nghiệm làm chủ những cung đường lớn.', 'img/pkl.jpg'),
(N'Xe điện', N'Giải pháp di chuyển của tương lai, hoàn toàn không phát thải khí nhà kính và vận hành cực kỳ êm ái. Chi phí vận hành vô cùng tiết kiệm, tích hợp nhiều tính năng thông minh và không yêu cầu bảo dưỡng động cơ phức tạp.', 'img/dien.jpg'),
(N'Xe cổ điển', N'Sở hữu thiết kế vượt thời gian, mang đậm nét hoài cổ (vintage) nhưng kết hợp cùng công nghệ động cơ hiện đại. Lựa chọn hoàn hảo cho những tâm hồn lãng mạn, yêu thích phong cách thời trang thanh lịch, phong trần và khác biệt.', 'img/classic.jpg'),
(N'Xe thể thao', N'Kiệt tác khí động học lấy cảm hứng từ các giải đua xe chuyên nghiệp. Đặc trưng bởi dàn áo yếm (fairing) góc cạnh, tư thế ngồi chồm về phía trước, cho khả năng bứt tốc kinh ngạc và độ bám đường hoàn hảo ở tốc độ cao.', 'img/sport.jpg'),
(N'Xe touring', N'Sinh ra để dành cho những chuyến hành trình xuyên quốc gia. Dòng xe được tối ưu hóa tối đa cho sự thoải mái với tư thế ngồi bệ vệ, yên xe êm ái, kính chắn gió lớn và hệ thống thùng chứa đồ dung tích khủng.', 'img/touring.jpg'),
(N'Xe mini', N'Dòng xe có thiết kế siêu nhỏ gọn, trọng lượng cực nhẹ và kiểu dáng vô cùng phá cách, cá tính. Rất linh hoạt khi luồn lách trong ngõ hẻm chật hẹp, là lựa chọn thú vị để dạo phố hoặc dành cho những người có vóc dáng khiêm tốn.', 'img/mini.jpg'),
(N'Xe nhập khẩu', N'Những mẫu xe nguyên chiếc được nhập khẩu trực tiếp từ các thị trường quốc tế (Thái Lan, Indonesia, Ý...). Nổi bật với tiêu chuẩn hoàn thiện khắt khe, thiết kế độc quyền và luôn được giới chơi xe săn đón nồng nhiệt.', 'img/import.jpg');
GO

-- ==========================================
-- 2. CHÈN DỮ LIỆU THƯƠNG HIỆU (Brands)
-- ==========================================
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
GO

-- ==========================================
-- 3. CHÈN DỮ LIỆU SẢN PHẨM (Products)
-- ==========================================
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
GO

-- ==========================================
-- 4. CHÈN MÀU SẮC BIẾN THỂ (ProductColors)
-- ==========================================
INSERT INTO ProductColors (product_id, color_name, price_adjustment) VALUES
(1, N'Đỏ Đen', 0), (1, N'Xanh Đậm', 0),
(2, N'Đen Nhám', 0), (2, N'Trắng Đỏ', 500000),
(3, N'Trắng', 0), (3, N'Đen Mờ', 1500000),
(4, N'Xanh Ngọc', 0), (4, N'Đỏ Đun', 0),
(5, N'Bạc Mờ', 1000000), (5, N'Đỏ Xám', 0),
(6, N'Xanh GP', 2000000), (6, N'Đen Nhám', 0),
(7, N'Đỏ Đen', 0), (7, N'Camo', 1000000),
(8, N'Xanh Đen', 0), (8, N'Xám Đen', 1000000),
(9, N'Đen Nhám', 0), (9, N'Cam Đen', 0),
(10, N'Trắng', 0), (10, N'Đen', 0),
(11, N'Vàng', 0), (11, N'Trắng', 0),
(12, N'Bạc', 2000000), (12, N'Đỏ', 0),
(13, N'Đen Nhám', 3000000), (13, N'Trắng Đen', 0),
(14, N'Xanh Trắng', 0), (14, N'Đen', 0),
(15, N'Đỏ Đen', 0), (15, N'Xanh', 0),
(16, N'Xanh Đen', 0), (16, N'Đỏ Đen', 0),
(17, N'Xanh GP', 0), (17, N'Đen', 0),
(18, N'Đỏ Đen', 0), (18, N'Đen Nhám', 0),
(19, N'Xanh GP', 0), (19, N'Đỏ Đen', 0),
(20, N'Đỏ', 0), (20, N'Xanh Đậm', 0),
(21, N'Đỏ Đen', 0), (21, N'Đen Nhám', 0),
(22, N'Trắng', 0), (22, N'Đỏ', 0),
(23, N'Xám', 0), (23, N'Trắng', 0),
(24, N'Xanh Nhạt', 0), (24, N'Trắng Đỏ', 0),
(25, N'Đỏ Ý', 0), (25, N'Đen', 0),
(26, N'Trắng Xanh', 0), (26, N'Đen', 0),
(27, N'Cam Đen', 0), (27, N'Trắng', 0),
(28, N'Đen Nhám', 0), (28, N'Xanh GP', 0),
(29, N'Đen', 0), (29, N'Bạc', 0),
(30, N'Trắng', 0), (30, N'Xanh Mờ', 0),
(31, N'Xám', 0), (31, N'Đen', 0),
(32, N'Đỏ Đen', 0), (32, N'Đen Trắng', 0),
(33, N'Xanh Trắng', 0), (33, N'Đỏ', 0),
(34, N'Vàng', 0), (34, N'Trắng', 0),
(35, N'Đỏ', 0), (35, N'Xanh Đậm', 0),
(36, N'Đen', 0), (36, N'Trắng', 0),
(37, N'Xanh KRT', 0), (37, N'Đen Nhám', 0),
(38, N'Đỏ Đặc Trưng', 0), (38, N'Đen', 0),
(39, N'Trắng Xanh Đỏ', 0), (39, N'Đen Mờ', 0),
(40, N'Cam', 0), (40, N'Trắng', 0),
(41, N'Xám Nhám', 0), (41, N'Đen Bóng', 0),
(42, N'Hồng', 0), (42, N'Trắng', 0),
(43, N'Đỏ', 0), (43, N'Trắng', 0),
(44, N'Nho Mờ', 0), (44, N'Đỏ', 0),
(45, N'Cam', 0), (45, N'Đen Trắng', 0),
(46, N'Vàng Đen', 0), (46, N'Trắng Xanh', 0),
(47, N'Xanh Lá', 0), (47, N'Đen', 0),
(48, N'Trắng', 0), (48, N'Đỏ', 0),
(49, N'Đen Nhám', 0), (49, N'Trắng', 0),
(50, N'Bạc', 0), (50, N'Đen', 0);
GO

-- ==========================================
-- 5. CHÈN TỒN KHO THEO MÀU (Inventory)
-- ==========================================
INSERT INTO Inventory (color_id, stock_quantity) VALUES
(1, 15), (2, 8), (3, 20), (4, 12), (5, 5), (6, 0), (7, 10), (8, 25), (9, 4), (10, 18),
(11, 7), (12, 14), (13, 22), (14, 0), (15, 30), (16, 11), (17, 9), (18, 16), (19, 5), (20, 2),
(21, 19), (22, 13), (23, 6), (24, 0), (25, 21), (26, 8), (27, 17), (28, 24), (29, 3), (30, 28),
(31, 12), (32, 9), (33, 1), (34, 0), (35, 15), (36, 10), (37, 22), (38, 7), (39, 4), (40, 19),
(41, 11), (42, 6), (43, 27), (44, 2), (45, 14), (46, 20), (47, 0), (48, 8), (49, 13), (50, 16),
(51, 5), (52, 23), (53, 9), (54, 18), (55, 30), (56, 12), (57, 1), (58, 26), (59, 10), (60, 4),
(61, 15), (62, 7), (63, 0), (64, 21), (65, 11), (66, 8), (67, 19), (68, 14), (69, 3), (70, 25),
(71, 17), (72, 6), (73, 29), (74, 2), (75, 13), (76, 22), (77, 9), (78, 0), (79, 16), (80, 24),
(81, 18), (82, 5), (83, 12), (84, 27), (85, 10), (86, 1), (87, 8), (88, 20), (89, 15), (90, 7),
(91, 23), (92, 4), (93, 11), (94, 0), (95, 26), (96, 14), (97, 19), (98, 3), (99, 21), (100, 9);
GO

-- ==========================================
-- 6. CHÈN DANH MỤC THÔNG SỐ (Specifications)
-- ==========================================
INSERT INTO Specifications (spec_name, display_order) VALUES
('Khối lượng bản thân', 1),
('Dài x Rộng x Cao', 2),
('Khoảng cách trục bánh xe', 3),
('Độ cao yên', 4),
('Khoảng sáng gầm xe', 5),
('Loại động cơ', 11),
('Dung tích xy-lanh', 12),
('Đường kính x Hành trình pít tông', 13),
('Tỷ số nén', 14),
('Công suất tối đa', 15),
('Moment cực đại', 16),
('Hệ thống làm mát', 17),
('Hệ thống khởi động', 18),
('Loại truyền động', 19),
('Dung tích bình xăng', 20),
('Dung tích nhớt máy', 21),
('Mức tiêu thụ nhiên liệu', 22),
('Loại động cơ điện', 31),
('Công suất danh định (Động cơ điện)', 32),
('Loại pin / Ắc-quy', 33),
('Dung lượng pin', 34),
('Thời gian sạc đầy', 35),
('Quãng đường di chuyển / 1 lần sạc', 36),
('Tốc độ tối đa', 37),
('Phuộc trước', 41),
('Phuộc sau', 42),
('Kích cỡ lốp trước', 43),
('Kích cỡ lốp sau', 44),
('Loại phanh trước', 45),
('Loại phanh sau', 46),
('Công nghệ an toàn (ABS/CBS/TCS...)', 47),
('Chế độ lái (Riding Modes)', 48),
('Màn hình đồng hồ', 49);
GO

-- ==========================================
-- 7. CHÈN CHI TIẾT THÔNG SỐ XE (ProductSpecifications)
-- ==========================================
INSERT INTO ProductSpecifications (product_id, spec_id, spec_value) VALUES
(1,1,N'97 kg'), (1,2,N'1.914 x 688 x 1.075 mm'), (1,3,N'769 mm'), (1,4,N'3,7 Lít'), (1,5,N'1,72 L/100km'), (1,6,N'Xăng, 4 kỳ, 1 xy-lanh, làm mát bằng không khí'), (1,7,N'109,2 cc'), (1,8,N'Cơ khí, 4 số tròn'), (1,9,N'Phanh cơ (Tang trống)'),
(2,1,N'99 kg'), (2,2,N'1.940 x 715 x 1.075 mm'), (2,3,N'775 mm'), (2,4,N'3,8 Lít'), (2,5,N'1,99 L/100km'), (2,6,N'Xăng, 4 kỳ, 2 van SOHC'), (2,7,N'114 cc'), (2,8,N'Cơ khí, 4 số tròn'), (2,9,N'Phanh đĩa / Phanh cơ'),
(3,1,N'94 kg'), (3,2,N'1.871 x 686 x 1.101 mm'), (3,3,N'761 mm'), (3,4,N'4,9 Lít'), (3,5,N'1,85 L/100km'), (3,6,N'eSP, 4 kỳ, làm mát bằng không khí'), (3,7,N'109,5 cc'), (3,8,N'Vô cấp CVT'), (3,9,N'Phanh đĩa CBS / Phanh cơ'),
(4,1,N'97 kg'), (4,2,N'1.850 x 705 x 1.120 mm'), (4,3,N'770 mm'), (4,4,N'4,2 Lít'), (4,5,N'1,87 L/100km'), (4,6,N'Blue Core, 4 kỳ, SOHC'), (4,7,N'125 cc'), (4,8,N'Vô cấp CVT'), (4,9,N'Phanh đĩa / Phanh cơ'),
(5,1,N'113 kg'), (5,2,N'1.844 x 680 x 1.130 mm'), (5,3,N'760 mm'), (5,4,N'6,0 Lít'), (5,5,N'2,16 L/100km'), (5,6,N'eSP+, 4 van, làm mát bằng dung dịch'), (5,7,N'124,8 cc'), (5,8,N'Vô cấp CVT'), (5,9,N'Phanh đĩa / Phanh cơ'),
(6,1,N'121 kg'), (6,2,N'1.975 x 665 x 1.085 mm'), (6,3,N'795 mm'), (6,4,N'5,4 Lít'), (6,5,N'2,09 L/100km'), (6,6,N'4 kỳ, 4 van, SOHC, VVA'), (6,7,N'155 cc'), (6,8,N'Côn tay, 6 cấp'), (6,9,N'Đĩa kép / Đĩa đơn (ABS)'),
(7,1,N'122 kg'), (7,2,N'2.019 x 727 x 1.104 mm'), (7,3,N'795 mm'), (7,4,N'4,5 Lít'), (7,5,N'1,99 L/100km'), (7,6,N'DOHC, 4 kỳ, xi-lanh đơn'), (7,7,N'149,1 cc'), (7,8,N'Côn tay, 6 cấp'), (7,9,N'Đĩa kép / Đĩa đơn (ABS)'),
(8,1,N'113 kg'), (8,2,N'1.887 x 687 x 1.092 mm'), (8,3,N'774 mm'), (8,4,N'4,4 Lít'), (8,5,N'2,26 L/100km'), (8,6,N'eSP+, 4 van, làm mát bằng dung dịch'), (8,7,N'156,9 cc'), (8,8,N'Vô cấp CVT'), (8,9,N'Đĩa ABS / Cơ'),
(9,1,N'125 kg'), (9,2,N'1.980 x 700 x 1.150 mm'), (9,3,N'790 mm'), (9,4,N'5,5 Lít'), (9,5,N'2,19 L/100km'), (9,6,N'Blue Core 4 kỳ, SOHC, VVA'), (9,7,N'155 cc'), (9,8,N'Vô cấp CVT'), (9,9,N'Đĩa ABS / Cơ'),
(10,1,N'120 kg'), (10,2,N'1.958 x 695 x 1.110 mm'), (10,3,N'780 mm'), (10,4,N'6,0 Lít'), (10,5,N'2,58 L/100km'), (10,6,N'iGet, xi-lanh đơn, 3 van'), (10,7,N'124,5 cc'), (10,8,N'Vô cấp CVT'), (10,9,N'Đĩa ABS / Cơ'),
(11,1,N'120 kg'), (11,2,N'1.863 x 695 x 1.150 mm'), (11,3,N'790 mm'), (11,4,N'7,0 Lít'), (11,5,N'2,63 L/100km'), (11,6,N'iGet 4 kỳ, 3 van'), (11,7,N'124,5 cc'), (11,8,N'Vô cấp CVT'), (11,9,N'Đĩa ABS / Cơ'),
(12,1,N'116 kg'), (12,2,N'1.950 x 669 x 1.100 mm'), (12,3,N'765 mm'), (12,4,N'5,6 Lít'), (12,5,N'2,16 L/100km'), (12,6,N'eSP+ 4 van'), (12,7,N'124,8 cc'), (12,8,N'Vô cấp CVT'), (12,9,N'Đĩa ABS / Cơ'),
(13,1,N'133 kg'), (13,2,N'2.090 x 739 x 1.129 mm'), (13,3,N'799 mm'), (13,4,N'7,8 Lít'), (13,5,N'2,24 L/100km'), (13,6,N'eSP+ 4 van, làm mát dung dịch'), (13,7,N'156,9 cc'), (13,8,N'Vô cấp CVT'), (13,9,N'Đĩa ABS kép'),
(14,1,N'94 kg'), (14,2,N'1.910 x 690 x 1.085 mm'), (14,3,N'760 mm'), (14,4,N'3,7 Lít'), (14,5,N'1,7 L/100km'), (14,6,N'4 kỳ, 1 xi-lanh'), (14,7,N'113 cc'), (14,8,N'Cơ khí, 4 số'), (14,9,N'Phanh cơ'),
(15,1,N'98 kg'), (15,2,N'1.940 x 695 x 1.095 mm'), (15,3,N'750 mm'), (15,4,N'4,0 Lít'), (15,5,N'1,7L/100km'), (15,6,N'4 kỳ, làm mát bằng không khí'), (15,7,N'114 cc'), (15,8,N'Cơ khí, 4 số'), (15,9,N'Đĩa / Cơ'),
(16,1,N'221 kg'), (16,2,N'2.045 x 790 x 1.055 mm'), (16,3,N'815 mm'), (16,4,N'17 Lít'), (16,5,N'5,8 L/100km'), (16,6,N'DOHC, 4 xi-lanh thẳng hàng'), (16,7,N'1.043 cc'), (16,8,N'Côn tay, 6 cấp'), (16,9,N'Đĩa kép ABS / Đĩa đơn ABS'),
(17,1,N'137 kg'), (17,2,N'1.990 x 725 x 1.135 mm'), (17,3,N'815 mm'), (17,4,N'11 Lít'), (17,5,N'2,02 L/100km'), (17,6,N'SOHC, 4 van, VVA'), (17,7,N'155 cc'), (17,8,N'Côn tay, 6 cấp'), (17,9,N'Đĩa / Đĩa (ABS)'),
(18,1,N'139 kg'), (18,2,N'1.983 x 700 x 1.090 mm'), (18,3,N'788 mm'), (18,4,N'12 Lít'), (18,5,N'2,91 L/100km'), (18,6,N'DOHC, 4 kỳ'), (18,7,N'149,2 cc'), (18,8,N'Côn tay, 6 cấp'), (18,9,N'Đĩa ABS trước sau'),
(19,1,N'109 kg'), (19,2,N'1.960 x 675 x 980 mm'), (19,3,N'765 mm'), (19,4,N'4,0 Lít'), (19,5,N'2,4 L/100km'), (19,6,N'DOHC, 4 van, làm mát dung dịch'), (19,7,N'147,3 cc'), (19,8,N'Côn tay, 6 cấp'), (19,9,N'Đĩa / Đĩa'),
(20,1,N'104 kg'), (20,2,N'1.931 x 711 x 1.083 mm'), (20,3,N'756 mm'), (20,4,N'4,6 Lít'), (20,5,N'1,54 L/100km'), (20,6,N'4 kỳ, 1 xi-lanh'), (20,7,N'124,9 cc'), (20,8,N'Cơ khí, 4 số'), (20,9,N'Đĩa / Cơ'),
(21,1,N'104 kg'), (21,2,N'1.935 x 680 x 1.065 mm'), (21,3,N'765 mm'), (21,4,N'4,1 Lít'), (21,5,N'1,55 L/100km'), (21,6,N'SOHC, 2 van'), (21,7,N'114 cc'), (21,8,N'Cơ khí, 4 số'), (21,9,N'Đĩa / Cơ'),
(22,1,N'108 kg'), (22,2,N'1.880 x 680 x 1.120 mm'), (22,3,N'750 mm'), (22,4,N'5,5 Lít'), (22,5,N'2,2 L/100km'), (22,6,N'4 kỳ, 1 xi-lanh'), (22,7,N'124,6 cc'), (22,8,N'Vô cấp CVT'), (22,9,N'Đĩa / Cơ'),
(23,1,N'115 kg'), (23,2,N'1.935 x 690 x 1.145 mm'), (23,3,N'790 mm'), (23,4,N'6,0 Lít'), (23,5,N'2,1 L/100km'), (23,6,N'4 kỳ, làm mát bằng gió'), (23,7,N'125 cc'), (23,8,N'Vô cấp CVT'), (23,9,N'Đĩa ABS / Đĩa'),
(24,1,N'129 kg'), (24,2,N'1.925 x 710 x 1.190 mm'), (24,3,N'770 mm'), (24,4,N'8,5 Lít'), (24,5,N'2,7 L/100km'), (24,6,N'EasyMotion 4 van'), (24,7,N'150 cc'), (24,8,N'Vô cấp CVT'), (24,9,N'Đĩa ABS / Đĩa'),
(25,1,N'166 kg'), (25,2,N'2.120 x 820 x 1.080 mm'), (25,3,N'820 mm'), (25,4,N'14 Lít'), (25,5,N'5,2 L/100km'), (25,6,N'Testastretta 11° V2'), (25,7,N'937 cc'), (25,8,N'Côn tay, 6 cấp (Quickshifter)'), (25,9,N'Đĩa kép Brembo ABS EVO / Đĩa đơn ABS'),
(26,1,N'164 kg'), (26,2,N'2.005 x 820 x 1.080 mm'), (26,3,N'785 mm'), (26,4,N'11 Lít'), (26,5,N'3,3 L/100km'), (26,6,N'Xi-lanh đơn, 4 thì, DOHC'), (26,7,N'313 cc'), (26,8,N'Côn tay, 6 cấp'), (26,9,N'Đĩa Bybre ABS'),
(27,1,N'150 kg'), (27,2,N'2.002 x 873 x 1.274 mm'), (27,3,N'830 mm'), (27,4,N'13,4 Lít'), (27,5,N'3,4 L/100km'), (27,6,N'Xi-lanh đơn, DOHC'), (27,7,N'373 cc'), (27,8,N'Côn tay, 6 cấp'), (27,9,N'Đĩa Bybre ABS'),
(28,1,N'133 kg'), (28,2,N'1.965 x 800 x 1.065 mm'), (28,3,N'810 mm'), (28,4,N'10,4 Lít'), (28,5,N'2,09 L/100km'), (28,6,N'SOHC, 4 van, VVA'), (28,7,N'155 cc'), (28,8,N'Côn tay, 6 cấp'), (28,9,N'Đĩa / Đĩa'),
(29,1,N'130 kg'), (29,2,N'1.923 x 745 x 1.107 mm'), (29,3,N'764 mm'), (29,4,N'8,0 Lít'), (29,5,N'2,0 L/100km'), (29,6,N'eSP, 4 kỳ'), (29,7,N'149,3 cc'), (29,8,N'Vô cấp CVT'), (29,9,N'Đĩa / Cơ'),
(30,1,N'100 kg'), (30,2,N'1.820 x 685 x 1.150 mm'), (30,3,N'790 mm'), (30,4,N'4,4 Lít'), (30,5,N'1,66 L/100km'), (30,6,N'Blue Core Hybrid'), (30,7,N'125 cc'), (30,8,N'Vô cấp CVT'), (30,9,N'Đĩa ABS / Cơ'),
(31,1,N'110 kg'), (31,2,N'1.880 x 715 x 1.140 mm'), (31,3,N'780 mm'), (31,4,N'5,5 Lít'), (31,5,N'1,9 L/100km'), (31,6,N'SEP, 4 kỳ, SOHC'), (31,7,N'125 cc'), (31,8,N'Vô cấp CVT'), (31,9,N'Đĩa / Cơ'),
(32,1,N'98 kg'), (32,2,N'1.920 x 702 x 1.075 mm'), (32,3,N'769 mm'), (32,4,N'3,7 Lít'), (32,5,N'1,85 L/100km'), (32,6,N'4 kỳ, 1 xi-lanh'), (32,7,N'109,1 cc'), (32,8,N'Cơ khí, 4 số'), (32,9,N'Đĩa / Cơ'),
(33,1,N'95 kg'), (33,2,N'1.910 x 680 x 1.070 mm'), (33,3,N'750 mm'), (33,4,N'4,0 Lít'), (33,5,N'1,4 L/100km'), (33,6,N'4 kỳ, 1 xi-lanh'), (33,7,N'49,5 cc'), (33,8,N'Cơ khí, 4 số'), (33,9,N'Cơ / Cơ'),
(34,1,N'97 kg'), (34,2,N'1.804 x 683 x 1.127 mm'), (34,3,N'750 mm'), (34,4,N'Pin LFP (Lithium Iron Phosphate)'), (34,5,N'203 km / 1 lần sạc'), (34,6,N'Động cơ Inhub'), (34,7,N'Khoảng 10 giờ'), (34,8,N'Truyền động trực tiếp'), (34,9,N'Đĩa / Cơ'),
(35,1,N'112 kg'), (35,2,N'1.895 x 690 x 1.130 mm'), (35,3,N'760 mm'), (35,4,N'Pin LFP'), (35,5,N'194 km / 1 lần sạc'), (35,6,N'Động cơ Inhub'), (35,7,N'Khoảng 6 giờ'), (35,8,N'Truyền động trực tiếp'), (35,9,N'Đĩa / Đĩa'),
(36,1,N'126 kg'), (36,2,N'1.912 x 693 x 1.128 mm'), (36,3,N'770 mm'), (36,4,N'Pin LFP'), (36,5,N'198 km / 1 lần sạc'), (36,6,N'Động cơ Inhub'), (36,7,N'Khoảng 6 giờ'), (36,8,N'Truyền động trực tiếp'), (36,9,N'Đĩa / Cơ'),
(37,1,N'168 kg'), (37,2,N'1.990 x 710 x 1.120 mm'), (37,3,N'785 mm'), (37,4,N'14 Lít'), (37,5,N'4,0 L/100km'), (37,6,N'DOHC, 2 xi-lanh song song'), (37,7,N'399 cc'), (37,8,N'Côn tay, 6 cấp'), (37,9,N'Đĩa kép ABS / Đĩa ABS'),
(38,1,N'193 kg'), (38,2,N'2.056 x 810 x 1.115 mm'), (38,3,N'835 mm'), (38,4,N'16 Lít'), (38,5,N'6,0 L/100km'), (38,6,N'Desmosedici Stradale 90° V4'), (38,7,N'1.103 cc'), (38,8,N'Côn tay, 6 cấp, DQS'), (38,9,N'Đĩa kép Brembo ABS EVO'),
(39,1,N'197 kg'), (39,2,N'2.073 x 848 x 1.151 mm'), (39,3,N'824 mm'), (39,4,N'16,5 Lít'), (39,5,N'6,4 L/100km'), (39,6,N'DOHC, 4 xi-lanh thẳng hàng, ShiftCam'), (39,7,N'999 cc'), (39,8,N'Côn tay, 6 cấp'), (39,9,N'Đĩa kép ABS Pro'),
(40,1,N'155 kg'), (40,2,N'1.978 x 748 x 1.098 mm'), (40,3,N'824 mm'), (40,4,N'13,7 Lít'), (40,5,N'3,46 L/100km'), (40,6,N'DOHC, xi-lanh đơn'), (40,7,N'373 cc'), (40,8,N'Côn tay, 6 cấp, Quickshifter+'), (40,9,N'Đĩa Bybre ABS'),
(41,1,N'160 kg'), (41,2,N'1.950 x 755 x 1.175 mm'), (41,3,N'790 mm'), (41,4,N'8,5 Lít'), (41,5,N'3,2 L/100km'), (41,6,N'HPE 4 kỳ, 4 van, làm mát dung dịch'), (41,7,N'278 cc'), (41,8,N'Vô cấp CVT'), (41,9,N'Đĩa kép ABS / ASR'),
(42,1,N'94 kg'), (42,2,N'1.864 x 683 x 1.075 mm'), (42,3,N'746 mm'), (42,4,N'4,2 Lít'), (42,5,N'1,8 L/100km'), (42,6,N'eSP, 4 kỳ'), (42,7,N'109,5 cc'), (42,8,N'Vô cấp CVT'), (42,9,N'Đĩa / Cơ'),
(43,1,N'100 kg'), (43,2,N'1.820 x 680 x 1.160 mm'), (43,3,N'790 mm'), (43,4,N'5,5 Lít'), (43,5,N'1,8 L/100km'), (43,6,N'Blue Core, 2 van'), (43,7,N'125 cc'), (43,8,N'Vô cấp CVT'), (43,9,N'Đĩa / Cơ'),
(44,1,N'99 kg'), (44,2,N'1.867 x 696 x 1.040 mm'), (44,3,N'745 mm'), (44,4,N'4,0 Lít'), (44,5,N'1,6 L/100km'), (44,6,N'4 kỳ, 1 xi-lanh'), (44,7,N'124,8 cc'), (44,8,N'Cơ khí, 4 số'), (44,9,N'Cơ / Cơ'),
(45,1,N'140 kg'), (45,2,N'2.002 x 873 x 1.274 mm'), (45,3,N'810 mm'), (45,4,N'13,4 Lít'), (45,5,N'2,8 L/100km'), (45,6,N'DOHC, xi-lanh đơn'), (45,7,N'199,5 cc'), (45,8,N'Côn tay, 6 cấp'), (45,9,N'Đĩa Bybre ABS'),
(46,1,N'169.5 kg'), (46,2,N'2.075 x 880 x 1.230 mm'), (46,3,N'835 mm'), (46,4,N'11,5 Lít'), (46,5,N'3,33 L/100km'), (46,6,N'DOHC, xi-lanh đơn'), (46,7,N'313 cc'), (46,8,N'Côn tay, 6 cấp'), (46,9,N'Đĩa Bybre ABS'),
(47,1,N'214 kg'), (47,2,N'2.165 x 840 x 1.400 mm'), (47,3,N'840 mm'), (47,4,N'21 Lít'), (47,5,N'4,5 L/100km'), (47,6,N'DOHC, 2 xi-lanh song song'), (47,7,N'649 cc'), (47,8,N'Côn tay, 6 cấp'), (47,9,N'Đĩa kép ABS / Đĩa ABS'),
(48,1,N'134 kg'), (48,2,N'1.890 x 735 x 1.115 mm'), (48,3,N'800 mm'), (48,4,N'6,0 Lít'), (48,5,N'2,9 L/100km'), (48,6,N'4 kỳ, làm mát bằng không khí'), (48,7,N'169 cc'), (48,8,N'Vô cấp CVT'), (48,9,N'Đĩa ABS / Đĩa'),
(49,1,N'133 kg'), (49,2,N'1.950 x 763 x 1.196 mm'), (49,3,N'780 mm'), (49,4,N'8,1 Lít'), (49,5,N'2,22 L/100km'), (49,6,N'eSP+, 4 van'), (49,7,N'156,9 cc'), (49,8,N'Vô cấp CVT'), (49,9,N'Đĩa ABS / Đĩa'),
(50,1,N'134 kg'), (50,2,N'2.000 x 805 x 1.080 mm'), (50,3,N'810 mm'), (50,4,N'10 Lít'), (50,5,N'2,0 L/100km'), (50,6,N'SOHC, 4 van, VVA'), (50,7,N'155 cc'), (50,8,N'Côn tay, 6 cấp'), (50,9,N'Đĩa / Đĩa');
GO