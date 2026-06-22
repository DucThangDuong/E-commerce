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
CREATE TABLE Products (
    product_id INT IDENTITY(1,1) PRIMARY KEY,
    category_id INT NOT NULL,
    brand_id INT,
    name NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
    base_price DECIMAL(18, 2) NOT NULL,
    is_deleted BIT default 0,
    FOREIGN KEY (category_id) REFERENCES Categories(category_id) ON DELETE CASCADE,
    FOREIGN KEY (brand_id) REFERENCES Brands(brand_id) ON DELETE CASCADE
);
GO

CREATE TABLE Orders (
    order_id INT IDENTITY(1,1) PRIMARY KEY,
    customer_id INT NOT NULL,
    order_date DATETIME DEFAULT GETDATE(),
    Updated_at DATETIME DEFAULT GETDATE(),
    coupon_id INT NULL,
    discount_amount DECIMAL(15, 2) DEFAULT 0,
    total_amount DECIMAL(18, 2) NOT NULL,
    status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (customer_id) REFERENCES Customers(customer_id) ON DELETE CASCADE,
    FOREIGN KEY (coupon_id) REFERENCES Coupons(coupon_id) ON DELETE SET NULL
);
GO

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
CREATE TABLE CancellationReasons (
    reason_id INT IDENTITY(1,1) PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    content NVARCHAR(255) NOT NULL,
    is_active BIT DEFAULT 1 NOT NULL,
    display_order INT DEFAULT 0 NOT NULL
);
GO
CREATE TABLE OrderCancellations (
    cancellation_id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL UNIQUE,
    reason_id INT NOT NULL,
    custom_reason_text NVARCHAR(500) NULL,
    canceled_at DATETIME DEFAULT GETDATE() NOT NULL,
    canceled_by_user_id INT NOT NULL,
    FOREIGN KEY (order_id) REFERENCES Orders(order_id) ON DELETE CASCADE,
    FOREIGN KEY (reason_id) REFERENCES CancellationReasons(reason_id) ON DELETE NO ACTION 
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


INSERT INTO Categories (name, description, picture) VALUES
(N'Xe số', N'Dòng xe phổ thông với thiết kế nhỏ gọn, động cơ bền bỉ và khả năng tiết kiệm nhiên liệu vượt trội. Phù hợp cho mọi nhu cầu từ đi học, đi làm đến chuyên chở hàng hóa hàng ngày. Dễ dàng bảo dưỡng với chi phí thấp nhất.', 'img/xe-so.jpg'),
(N'Xe tay ga', N'Mang đến sự tiện lợi tối đa cho việc di chuyển trong đô thị đông đúc. Sở hữu thiết kế thời trang, cốp chứa đồ siêu rộng, thao tác vận hành đơn giản cùng nhiều tiện ích công nghệ hiện đại đi kèm như khóa thông minh, phanh ABS.', 'img/xe-tay-ga.jpg'),
(N'Xe côn tay', N'Dòng xe mang đậm phong cách thể thao, dành cho những ai đam mê tốc độ và muốn làm chủ hoàn toàn sức mạnh động cơ. Thao tác bóp côn gảy số mang lại cảm giác lái phấn khích, khả năng tăng tốc ấn tượng và linh hoạt.', 'img/xe-con.jpg'),
(N'Xe PKL', N'Những cỗ máy sức mạnh mang dung tích xy-lanh từ 175cc trở lên. Đây là biểu tượng của đẳng cấp, tốc độ và sự tự do. Âm thanh ống xả uy lực cùng loạt công nghệ hỗ trợ lái tiên tiến nhất, mang đến trải nghiệm làm chủ những cung đường lớn.', 'img/pkl.jpg'),
(N'Xe điện', N'Giải pháp di chuyển của tương lai, hoàn toàn không phát thải khí nhà kính và vận hành cực kỳ êm ái. Chi phí vận hành vô cùng tiết kiệm, tích hợp nhiều tính năng thông minh và không yêu cầu bảo dưỡng động cơ phức tạp.', 'img/dien.jpg');

INSERT INTO brands (description, logo_url, name) VALUES(N'Hãng xe Nhật Bản', 'https://imageshare13.blob.core.windows.net/logo/honda.png', 'Honda');
INSERT INTO brands (description, logo_url, name) VALUES(N'Hãng xe thể thao', 'https://imageshare13.blob.core.windows.net/logo/yamaha.webp', 'Yamaha');
INSERT INTO brands (description, logo_url, name) VALUES(N'Xe bền bỉ', 'https://imageshare13.blob.core.windows.net/logo/suzuki.png', 'Suzuki');
INSERT INTO brands (description, logo_url, name) VALUES(N'Giá rẻ', 'https://imageshare13.blob.core.windows.net/logo/sym.webp', 'SYM');
INSERT INTO brands (description, logo_url, name) VALUES(N'Phong cách Ý', 'https://imageshare13.blob.core.windows.net/logo/piaggio.png', 'Piaggio');
INSERT INTO brands (description, logo_url, name) VALUES(N'PKL mạnh mẽ', 'https://imageshare13.blob.core.windows.net/logo/kawasaki.png', 'Kawasaki');
INSERT INTO brands (description, logo_url, name) VALUES(N'Xe Ý cao cấp', 'https://imageshare13.blob.core.windows.net/logo/ducati.png', 'Ducati');
INSERT INTO brands (description, logo_url, name) VALUES(N'Xe Đức', 'https://imageshare13.blob.core.windows.net/logo/BMW.jpg', 'BMW');
INSERT INTO brands (description, logo_url, name) VALUES(N'Thể thao', 'https://imageshare13.blob.core.windows.net/logo/KTM.png', 'KTM');
INSERT INTO brands (description, logo_url, name) VALUES(N'Xe Việt Nam', 'https://imageshare13.blob.core.windows.net/logo/Vinfast.png', 'VinFast');

INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(18000000.00, N'Mẫu xe số quốc dân với thiết kế nhỏ gọn, động cơ bền bỉ và khả năng tiết kiệm nhiên liệu vượt trội, sự lựa chọn hoàn hảo cho nhu cầu đi lại hàng ngày.', 0, 'Honda Wave Alpha', 1, 1);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(19000000.00, N'Chiếc xe số bền bỉ mang kiểu dáng thể thao, trẻ trung, động cơ mạnh mẽ và linh hoạt trên mọi địa hình, rất được các bạn trẻ ưa chuộng.', 0, 'Yamaha Sirius', 2, 1);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(31000000.00, N'Dòng xe tay ga phổ biến nhất với thiết kế thời trang, thanh lịch, trọng lượng nhẹ và tiện ích tối ưu, cực kỳ phù hợp cho phái nữ.', 0, 'Honda Vision', 1, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(29000000.00, N'Mẫu xe tay ga nhỏ gọn, trọng lượng nhẹ với thiết kế vuốt cao tôn dáng, mang lại trải nghiệm lái năng động và trẻ trung cho người dùng đô thị.', 0, 'Yamaha Janus', 2, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(39000000.00, N'Chiếc xe tay ga huyền thoại với thiết kế cốp siêu rộng lên đến 37 lít, động cơ eSP+ mạnh mẽ, là trợ thủ đắc lực cho phái đẹp và gia đình.', 0, 'Honda Lead', 1, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(47000000.00, N'Ông vua côn tay đường phố với khối động cơ 155 VVA mạnh mẽ, thiết kế khí động học chuẩn thể thao, mang lại cảm giác lái đầy phấn khích.', 0, 'Yamaha Exciter 155', 2, 3);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(46000000.00, N'Mẫu xe côn tay thể thao mang diện mạo hầm hố, trang bị phanh ABS an toàn cùng động cơ DOHC uy lực, sẵn sàng bứt phá mọi giới hạn.', 0, 'Honda Winner X', 1, 3);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(56000000.00, N'Dòng xe tay ga mang thiết kế góc cạnh, mạnh mẽ đầy nam tính, kết hợp khối động cơ eSP êm ái và loạt tiện ích hiện đại hàng đầu.', 0, 'Honda Air Blade', 1, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(54000000.00, N'Siêu xe tay ga thể thao với thiết kế hoành tráng, lốp to bản bám đường cực tốt và động cơ Blue Core 155cc uy lực, dành riêng cho phái mạnh.', 0, 'Yamaha NVX', 2, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(58000000.00, N'Mẫu xe tay ga cao cấp mang phong cách châu Âu thanh lịch, trang bị phanh ABS an toàn cùng những đường nét thiết kế tinh tế đầy cuốn hút.', 0, 'Piaggio Liberty', 5, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(75000000.00, N'Biểu tượng thời trang mang đậm phong cách Ý, kết hợp giữa nét cổ điển và động cơ iGet vận hành êm ái, thể hiện đẳng cấp người lái.', 0, 'Vespa Sprint', 5, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(58000000.00, N'Chiếc tay ga sang trọng với thiết kế mềm mại, tôn dáng quyến rũ, đi kèm động cơ eSP+ và nhiều tính năng hiện đại chuẩn phong cách châu Âu.', 0, 'Honda SH Mode', 1, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(92000000.00, N'Dòng xe tay ga cao cấp bậc nhất với thiết kế bề thế, động cơ 160cc mạnh mẽ vượt trội và công nghệ kết nối bluetooth thông minh hiện đại.', 0, 'Honda SH160', 1, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(21000000.00, N'Mẫu xe số huyền thoại nổi tiếng với khả năng tiết kiệm xăng vô địch, động cơ Fi vận hành bền bỉ, mang lại giá trị kinh tế cao cho người dùng.', 0, 'Suzuki Viva', 3, 1);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(17000000.00, N'Dòng xe số giá rẻ nhưng sở hữu thiết kế thể thao vuốt nhọn cá tính, động cơ 110cc bốc khỏe, rất phù hợp cho học sinh và sinh viên.', 0, 'SYM Galaxy', 4, 1);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(400000000.00, N'Siêu naked-bike phân khối lớn mạnh mẽ với thiết kế Sugomi dữ dằn, khối động cơ 1043cc uy lực, mang đến âm thanh ống xả đầy gầm gừ phấn khích.', 0, 'Kawasaki Z1000', 6, 4);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(70000000.00, N'Mẫu sportbike cỡ nhỏ với thiết kế thừa hưởng từ đàn anh R1, tư thế lái đậm chất thể thao và động cơ 155 VVA linh hoạt trên phố lẫn đường đua.', 0, 'Yamaha R15', 2, 3);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(72000000.00, N'Dòng xe thể thao mang cảm hứng từ xe đua đường phố, thiết kế khí động học sắc nét cùng tư thế chồm vừa phải, dễ dàng làm quen cho người mới.', 0, 'Honda CBR150R', 1, 3);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(50000000.00, N'Ông vua tốc độ trong phân khúc underbone với thiết kế hyper-underbone độc đáo, khối động cơ DOHC 150cc cho khả năng bứt tốc đáng kinh ngạc.', 0, 'Suzuki Raider', 3, 3);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(32000000.00, N'Mẫu xe số cao cấp mang thiết kế sang trọng lịch lãm như xe tay ga, cốp rộng rãi và khối động cơ 125cc phun xăng điện tử siêu tiết kiệm.', 0, 'Honda Future', 1, 1);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(30000000.00, N'Chiếc xe số thể thao mang sức mạnh vượt trội, khả năng tăng tốc ấn tượng và thiết kế đầu đèn đôi sắc sảo, thỏa mãn đam mê tốc độ.', 0, 'Yamaha Jupiter', 2, 1);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(35000000.00, N'Mẫu xe tay ga tiện lợi mang phong cách vintage nhẹ nhàng, sàn để chân phẳng rộng rãi và cốp lớn, sự lựa chọn tối ưu cho những chuyến đi dạo phố.', 0, 'SYM Attila', 4, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(36000000.00, N'Dòng xe tay ga cổ điển mang âm hưởng châu Âu, thiết kế nhỏ nhắn vát cong tinh tế, vận hành êm ái và cực kỳ phù hợp cho vóc dáng người Việt.', 0, 'Kymco Like', 4, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(68000000.00, N'Chiếc xe tay ga đậm chất lãng mạn của Pháp, sở hữu thiết kế retro hai tông màu độc đáo cùng tư thế ngồi thoải mái, sang trọng và khác biệt.', 0, 'Peugeot Django', 5, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(410000000.00, N'Quái vật đường phố mang dòng máu phân khối lớn của Ý, nổi bật với khung sườn mắt cáo trứ danh và sức mạnh động cơ L-Twin bùng nổ đầy hoang dại.', 0, 'Ducati Monster', 7, 4);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(150000000.00, N'Chiếc naked-bike đến từ thương hiệu xe Đức danh tiếng, mang lại trải nghiệm lái lanh lẹ, linh hoạt trong phố đô thị với chất lượng hoàn thiện tuyệt hảo.', 0, 'BMW G310R', 8, 4);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(160000000.00, N'Mẫu xe thể thao đường phố với trọng lượng nhẹ, khối động cơ xi lanh đơn bốc lửa, sẵn sàng mang đến những cú thốc ga phấn khích tột độ.', 0, 'KTM Duke 390', 9, 4);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(78000000.00, N'Mẫu naked bike đường phố mang DNA của gia đình MT series, thiết kế đầu đèn gương cầu ấn tượng và phuộc USD hành trình ngược đậm chất chơi.', 0, 'Yamaha MT15', 2, 3);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(88000000.00, N'Dòng tay ga cao cấp mang phong cách cruiser đường trường, tư thế ngồi cực kỳ thư giãn, rất phù hợp cho những chuyến đi phượt xa êm ái.', 0, 'Honda PCX', 1, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(46000000.00, N'Mẫu xe tay ga mang thiết kế thanh lịch chuẩn phong cách châu Âu, tự hào là một trong những dòng xe tiết kiệm nhiên liệu nhất thị trường Việt Nam.', 0, 'Yamaha Grande', 2, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(49000000.00, N'Dòng xe touring scooter với thiết kế bệ vệ hoành tráng, yên xe to bản êm ái, mang lại trải nghiệm tiện nghi tối đa trên mọi cung đường dài.', 0, 'Suzuki Burgman', 3, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(18500000.00, N'Mẫu xe số giá rẻ mang thiết kế thon gọn, liền mạch và khối động cơ 110cc mạnh mẽ vừa đủ, đáp ứng hoàn hảo nhu cầu di chuyển cơ bản mỗi ngày.', 0, 'Honda Blade', 1, 1);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(16000000.00, N'Chiếc xe số phân khối nhỏ 50cc gọn nhẹ, không cần bằng lái, thiết kế tem xe bắt mắt, là người bạn đồng hành lý tưởng cho lứa tuổi học sinh.', 0, 'SYM Elegant', 4, 1);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(22000000.00, N'Mẫu xe máy điện quốc dân với thiết kế hiện đại, quãng đường di chuyển lên tới 200km mỗi lần sạc, giải pháp di chuyển xanh tối ưu và thân thiện.', 0, 'VinFast Evo200', 10, 5);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(39000000.00, N'Dòng xe máy điện cao cấp mang vóc dáng thanh lịch phong cách Ý, trang bị kết nối thông minh và khả năng lội nước vượt trội trong điều kiện ngập úng.', 0, 'VinFast Klara', 10, 5);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(30000000.00, N'Chiếc xe máy điện sở hữu cốp siêu rộng, thiết kế trang nhã và chi phí vận hành siêu tiết kiệm, là lựa chọn thông minh cho dân văn phòng đô thị.', 0, 'VinFast Feliz', 10, 5);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(180000000.00, N'Mẫu sportbike tầm trung hoàn hảo cho người mới chơi phân khối lớn, thiết kế sắc sảo, tư thế ngồi thoải mái và động cơ xi lanh đôi êm ái.', 0, 'Kawasaki Ninja 400', 6, 4);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(700000000.00, N'Siêu xe thể thao đỉnh cao công nghệ mang thiết kế tuyệt mỹ, sức mạnh đường đua khủng khiếp, là niềm ao ước của mọi tín đồ đam mê tốc độ.', 0, 'Ducati Panigale', 7, 4);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(800000000.00, N'Siêu mô tô mang biệt danh cá mập với cặp đèn pha bất đối xứng trứ danh, tích hợp hàng loạt công nghệ điện tử hàng đầu thế giới dành cho đường đua.', 0, 'BMW S1000RR', 8, 4);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(170000000.00, N'Mẫu sportbike cá tính với bộ khung sườn cam nổi bật, thiết kế khí động học sắc bén, mang lại cảm giác lái cực kỳ thể thao và linh hoạt ở các góc cua.', 0, 'KTM RC390', 9, 4);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(95000000.00, N'Dòng xe tay ga thân lớn cao cấp bậc nhất của Vespa, sở hữu khối động cơ mạnh mẽ, hệ thống làm mát bằng dung dịch mang lại sự bền bỉ trên hành trình dài.', 0, 'Vespa GTS', 5, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(38000000.00, N'Mẫu tay ga nhập khẩu cỡ nhỏ mang thiết kế tròn trịa dễ thương, trang bị đèn LED hiện đại và cốp U-box tiện dụng, cực kỳ thu hút giới trẻ sành điệu.', 0, 'Honda Scoopy', 1, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(38000000.00, N'Chiếc xe tay ga thiết kế dành riêng cho nữ giới với cốp rộng 37 lít, nắp bình xăng tiện lợi phía trước và trọng lượng nhẹ, giúp chị em dễ dàng điều khiển.', 0, 'Yamaha Latte', 2, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(25000000.00, N'Huyền thoại xe số một thời với thiết kế vuông vức cổ điển, động cơ siêu bền bỉ thách thức thời gian, mang lại nhiều giá trị hoài niệm vô giá.', 0, 'Honda Dream', 1, 1);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(120000000.00, N'Mẫu xe thể thao cỡ nhỏ mang thiết kế góc cạnh sắc sảo, động cơ bốc và hệ thống treo WP xịn sò, phù hợp cho những ai đam mê sự khác biệt.', 0, 'KTM Duke 200', 9, 3);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(170000000.00, N'Chiếc xe adventure cỡ nhỏ mang thiết kế DNA của dòng GS huyền thoại, tư thế lái bệ vệ, sẵn sàng chinh phục những cung đường việt dã nhẹ nhàng.', 0, 'BMW G310GS', 8, 3);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(300000000.00, N'Dòng xe touring phân khối lớn với kính chắn gió cao, tư thế ngồi thẳng lưng thoải mái và động cơ êm ái, sinh ra để dành cho những chuyến đi xuyên Việt.', 0, 'Kawasaki Versys', 6, 4);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(86000000.00, N'Chiếc tay ga mang phong cách cổ điển của Ý, sở hữu đường nét thiết kế vuông vức đặc trưng kết hợp công nghệ hiện đại, mang đậm dấu ấn cá nhân.', 0, 'Lambretta V200', 5, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(90000000.00, N'Mẫu xe tay ga đường trường mang phong cách địa hình cứng cáp, trang bị phuộc Showa cao cấp và kính chắn gió điều chỉnh, sẵn sàng khám phá mọi địa hình.', 0, 'Honda ADV160', 1, 2);
INSERT INTO products (base_price, description, is_deleted, name, brand_id, category_id) VALUES(75000000.00, N'Chiếc xe côn tay mang phong cách Neo-Retro cổ điển pha lẫn hiện đại, trang bị động cơ 155 VVA và bộ ly hợp chống trượt, khơi nguồn cảm hứng tự do.', 0, 'Yamaha XSR155', 2, 3);

INSERT INTO ProductColors (product_id, color_name, price_adjustment) VALUES
(1, N'Đỏ Đen', 0), (1, N'Xanh Đậm', 0),
(2, N'Đen Nhám', 0), (2, N'Trắng Ngọc', 500000),
(3, N'Trắng', 0), (3, N'Đen Mờ', 1500000),
(4, N'Xanh Ngọc', 0), (4, N'Đỏ Đun', 0),
(5, N'Bạc Mờ', 1000000), (5, N'Đỏ Xám', 0),
(6, N'Xanh GP', 2000000), (6, N'Đen Nhám', 0),
(7, N'Đỏ Đen', 0), (7, N'Camo', 1000000),
(8, N'Xám Đen', 0), (8, N'Xanh Đen', 1000000),
(9, N'Đen Nhám', 0), (9, N'Trắng Nhám', 0),
(10, N'Trắng', 0), (10, N'Xanh Ngọc', 0),
(11, N'Trắng', 0), (11, N'Xanh', 0),
(12, N'Bạc', 2000000), (12, N'Đỏ', 0),
(13, N'Đen Nhám', 3000000), (13, N'Trắng Nhám', 0),
(14, N'Đen', 0), (14, N'Xanh Đen', 0),
(15, N'Xanh Nhám', 0), (15, N'Xanh', 0),
(16, N'Xanh Đen', 0), (16, N'Đen', 0),
(17, N'Xanh GP', 0), (17, N'Đen', 0),
(18, N'Đen Nhám', 0), (18, N'Đỏ Đen', 0),
(19, N'Đỏ Vàng', 0), (19, N'Xanh GP', 0),
(20, N'Đỏ', 0), (20, N'Xanh Đậm', 0),
(21, N'Đỏ Đen', 0), (21, N'Đen Nhám', 0),
(22, N'Đỏ', 0), (22, N'Trắng', 0),
(23, N'Xám', 0), (23, N'Trắng', 0),
(24, N'Đen Nhạt', 0), (24, N'Trắng Vàng', 0),
(25, N'Đen', 0), (25, N'Đỏ', 0),
(26, N'Đỏ', 0), (26, N'Xanh', 0),
(27, N'Cam Trắng', 0), (27, N'Cam Đen', 0),
(28, N'Xanh GP', 0), (28, N'Đen Nhám', 0),
(29, N'Đen', 0), (29, N'Xanh', 0),
(30, N'Xanh Mờ', 0), (30, N'Đen', 0),
(31, N'Trắng', 0), (31, N'Đen', 0),
(32, N'Trắng', 0), (32, N'Đỏ Đen', 0),
(33, N'Xanh', 0), (33, N'Đen', 0),
(34, N'Đen', 0), (34, N'Đỏ', 0),
(35, N'Xanh Đậm', 0), (35, N'Đỏ', 0),
(36, N'Trắng', 0), (36, N'Xanh Đậm', 0),
(37, N'Đen Nhám', 0), (37, N'Xanh KRT', 0),
(38, N'Đỏ', 0), (38, N'Đen', 0),
(39, N'Trắng Bạc', 0), (39, N'Đen Mờ', 0),
(40, N'Cam', 0), (40, N'Trắng', 0),
(41, N'Trắng', 0), (41, N'Xanh', 0),
(42, N'Trắng', 0), (42, N'Hồng', 0),
(43, N'Trắng', 0), (43, N'Xanh Đậm', 0),
(44, N'Đỏ', 0), (44, N'Nho Mờ', 0),
(45, N'Cam', 0), (45, N'Đen Trắng', 0),
(46, N'Đen Trắng', 0), (46, N'Xám Đen', 0),
(47, N'Xanh Lá', 0), (47, N'Đen Trắng', 0),
(48, N'Xanh', 0), (48, N'Đen', 0),
(49, N'Đen', 0), (49, N'Đen Cam', 0),
(50, N'Đen', 0), (50, N'Bạc', 0);

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

INSERT INTO Specifications (spec_name, display_order) VALUES
(N'Khối lượng bản thân', 1),
(N'Dài x Rộng x Cao', 2),
(N'Khoảng cách trục bánh xe', 3),
(N'Độ cao yên', 4),
(N'Khoảng sáng gầm xe', 5),
(N'Loại động cơ', 11),
(N'Dung tích xy-lanh', 12),
(N'Đường kính x Hành trình pít tông', 13),
(N'Tỷ số nén', 14),
(N'Công suất tối đa', 15),
(N'Moment cực đại', 16),
(N'Hệ thống làm mát', 17),
(N'Hệ thống khởi động', 18),
(N'Loại truyền động', 19),
(N'Dung tích bình xăng', 20),
(N'Dung tích nhớt máy', 21),
(N'Mức tiêu thụ nhiên liệu', 22),
(N'Loại động cơ điện', 31),
(N'Công suất danh định (Động cơ điện)', 32),
(N'Loại pin / Ắc-quy', 33),
(N'Dung lượng pin', 34),
(N'Thời gian sạc đầy', 35),
(N'Quãng đường di chuyển / 1 lần sạc', 36),
(N'Tốc độ tối đa', 37),
(N'Phuộc trước', 41),
(N'Phuộc sau', 42),
(N'Kích cỡ lốp trước', 43),
(N'Kích cỡ lốp sau', 44),
(N'Loại phanh trước', 45),
(N'Loại phanh sau', 46),
(N'Công nghệ an toàn (ABS/CBS/TCS...)', 47),
(N'Chế độ lái (Riding Modes)', 48),
(N'Màn hình đồng hồ', 49);

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
(25,1,N'166 kg'), (25,2,N'2.120 x 820 x 1.080 mm'), (25,3,N'820 mm'), (25,4,N'14 Lít'), (25,5,N'5,2 L/100km'), (25,6,N'Testastretta 11° V2'), (25,7,N'937 cc'), (25,8,N'Côn tay, 6 cấp (Quickshifter)'), (25,9,N'Đĩa kép Brembo ABS / Đĩa đơn ABS'),
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

INSERT INTO productimages (is_primary,image_url,display_order, uploaded_at, product_id, color_id)
VALUES
(NULL, 'https://imageshare13.blob.core.windows.net/products/1/1.png', 1, NULL, 1, 1),
(NULL, 'https://imageshare13.blob.core.windows.net/products/1/2.jpeg', 1, NULL, 1, 2),
(NULL, 'https://imageshare13.blob.core.windows.net/products/1/item1.png', 0, NULL, 1, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/1/item2.png', 0, NULL, 1, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/1/item3.png', 0, NULL, 1, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/1/item4.png', 0, NULL, 1, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/2/3.webp', 1, NULL, 2, 3),
(NULL, 'https://imageshare13.blob.core.windows.net/products/2/4.webp', 1, NULL, 2, 4),
(NULL, 'https://imageshare13.blob.core.windows.net/products/2/item1.jpg', 0, NULL, 2, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/2/item2.jpg', 0, NULL, 2, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/2/item3.jpg', 0, NULL, 2, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/2/item4.jpg', 0, NULL, 2, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/3/5.png', 1, NULL, 3, 5),
(NULL, 'https://imageshare13.blob.core.windows.net/products/3/6.png', 1, NULL, 3, 6),
(NULL, 'https://imageshare13.blob.core.windows.net/products/3/item1.png', 0, NULL, 3, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/3/item2.jpg', 0, NULL, 3, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/3/item3.png', 0, NULL, 3, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/3/item4.jpg', 0, NULL, 3, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/4/7.jpg', 1, NULL, 4, 7),
(NULL, 'https://imageshare13.blob.core.windows.net/products/4/8.jpg', 1, NULL, 4, 8),
(NULL, 'https://imageshare13.blob.core.windows.net/products/4/item1.jpg', 0, NULL, 4, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/4/item2.jpg', 0, NULL, 4, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/4/item3.jpg', 0, NULL, 4, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/4/item4.jpg', 0, NULL, 4, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/5/9.png', 1, NULL, 5, 9),
(NULL, 'https://imageshare13.blob.core.windows.net/products/5/10.png', 1, NULL, 5, 10),
(NULL, 'https://imageshare13.blob.core.windows.net/products/5/item1.jpg', 0, NULL, 5, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/5/item2.jpg', 0, NULL, 5, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/5/item3.jpg', 0, NULL, 5, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/5/item4.jpg', 0, NULL, 5, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/6/11.jpg', 1, NULL, 6, 11),
(NULL, 'https://imageshare13.blob.core.windows.net/products/6/12.jpg', 1, NULL, 6, 12),
(NULL, 'https://imageshare13.blob.core.windows.net/products/6/item1.webp', 0, NULL, 6, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/6/item2.webp', 0, NULL, 6, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/6/item3.webp', 0, NULL, 6, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/6/item4.webp', 0, NULL, 6, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/7/13.jpg', 1, NULL, 7, 13),
(NULL, 'https://imageshare13.blob.core.windows.net/products/7/14.png', 1, NULL, 7, 14),
(NULL, 'https://imageshare13.blob.core.windows.net/products/7/item1.jpg', 0, NULL, 7, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/7/item2.jpg', 0, NULL, 7, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/7/item3.jpg', 0, NULL, 7, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/7/item4.jpg', 0, NULL, 7, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/8/15.webp', 1, NULL, 8, 15),
(NULL, 'https://imageshare13.blob.core.windows.net/products/8/16.jpg', 1, NULL, 8, 16),
(NULL, 'https://imageshare13.blob.core.windows.net/products/8/item1.png', 0, NULL, 8, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/8/item2.png', 0, NULL, 8, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/8/item3.png', 0, NULL, 8, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/8/item4.png', 0, NULL, 8, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/9/17.jpg', 1, NULL, 9, 17),
(NULL, 'https://imageshare13.blob.core.windows.net/products/9/18.jpg', 1, NULL, 9, 18),
(NULL, 'https://imageshare13.blob.core.windows.net/products/9/item1.jpg', 0, NULL, 9, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/9/item2.jpg', 0, NULL, 9, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/9/item3.jpg', 0, NULL, 9, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/9/item4.jpg', 0, NULL, 9, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/10/19.avif', 1, NULL, 10, 19),
(NULL, 'https://imageshare13.blob.core.windows.net/products/10/20.avif', 1, NULL, 10, 20),
(NULL, 'https://imageshare13.blob.core.windows.net/products/10/item1.avif', 0, NULL, 10, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/10/item2.avif', 0, NULL, 10, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/10/item3.webp', 0, NULL, 10, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/10/item4.avif', 0, NULL, 10, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/11/21.avif', 1, NULL, 11, 21),
(NULL, 'https://imageshare13.blob.core.windows.net/products/11/22.avif', 1, NULL, 11, 22),
(NULL, 'https://imageshare13.blob.core.windows.net/products/11/item1.webp', 0, NULL, 11, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/11/item2.avif', 0, NULL, 11, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/11/item3.avif', 0, NULL, 11, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/11/item4.webp', 0, NULL, 11, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/12/23.png', 1, NULL, 12, 23),
(NULL, 'https://imageshare13.blob.core.windows.net/products/12/24.png', 1, NULL, 12, 24),
(NULL, 'https://imageshare13.blob.core.windows.net/products/12/item1.png', 0, NULL, 12, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/12/item2.png', 0, NULL, 12, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/12/item3.png', 0, NULL, 12, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/12/item4.png', 0, NULL, 12, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/13/25.webp', 1, NULL, 13, 25),
(NULL, 'https://imageshare13.blob.core.windows.net/products/13/26.png', 1, NULL, 13, 26),
(NULL, 'https://imageshare13.blob.core.windows.net/products/13/item1.png', 0, NULL, 13, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/13/item2.png', 0, NULL, 13, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/13/item3.png', 0, NULL, 13, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/13/item4.png', 0, NULL, 13, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/14/27.png', 1, NULL, 14, 27),
(NULL, 'https://imageshare13.blob.core.windows.net/products/14/28.png', 1, NULL, 14, 28),
(NULL, 'https://imageshare13.blob.core.windows.net/products/14/item1.jpg', 0, NULL, 14, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/14/item2.jpg', 0, NULL, 14, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/14/item3.jpg', 0, NULL, 14, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/14/item4.jpg', 0, NULL, 14, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/15/29.png', 1, NULL, 15, 29),
(NULL, 'https://imageshare13.blob.core.windows.net/products/15/30.png', 1, NULL, 15, 30),
(NULL, 'https://imageshare13.blob.core.windows.net/products/15/item1.jpg', 0, NULL, 15, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/15/item2.jpg', 0, NULL, 15, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/15/item3.jpg', 0, NULL, 15, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/15/item4.jpg', 0, NULL, 15, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/16/31.jpg', 1, NULL, 16, 31),
(NULL, 'https://imageshare13.blob.core.windows.net/products/16/32.jpg', 1, NULL, 16, 32),
(NULL, 'https://imageshare13.blob.core.windows.net/products/16/item1.jpg', 0, NULL, 16, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/16/item2.jpg', 0, NULL, 16, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/16/item3.jpg', 0, NULL, 16, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/16/item4.jpg', 0, NULL, 16, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/17/33.jpg', 1, NULL, 17, 33),
(NULL, 'https://imageshare13.blob.core.windows.net/products/17/34.jpg', 1, NULL, 17, 34),
(NULL, 'https://imageshare13.blob.core.windows.net/products/17/item1.jpg', 0, NULL, 17, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/17/item2.jpg', 0, NULL, 17, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/17/item3.jpg', 0, NULL, 17, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/17/item4.jpg', 0, NULL, 17, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/18/35.jpg', 1, NULL, 18, 35),
(NULL, 'https://imageshare13.blob.core.windows.net/products/18/36.webp', 1, NULL, 18, 36),
(NULL, 'https://imageshare13.blob.core.windows.net/products/18/item1.png', 0, NULL, 18, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/18/item2.png', 0, NULL, 18, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/18/item3.png', 0, NULL, 18, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/18/item4.png', 0, NULL, 18, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/19/37.jpg', 1, NULL, 19, 37),
(NULL, 'https://imageshare13.blob.core.windows.net/products/19/38.jpg', 1, NULL, 19, 38),
(NULL, 'https://imageshare13.blob.core.windows.net/products/19/item1.png', 0, NULL, 19, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/19/item2.jpg', 0, NULL, 19, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/19/item3.png', 0, NULL, 19, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/19/item4.jpg', 0, NULL, 19, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/20/39.jpg', 1, NULL, 20, 39),
(NULL, 'https://imageshare13.blob.core.windows.net/products/20/40.jpg', 1, NULL, 20, 40),
(NULL, 'https://imageshare13.blob.core.windows.net/products/20/item1.png', 0, NULL, 20, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/20/item2.jpg', 0, NULL, 20, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/20/item3.png', 0, NULL, 20, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/20/item4.jpg', 0, NULL, 20, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/21/41.jpg', 1, NULL, 21, 41),
(NULL, 'https://imageshare13.blob.core.windows.net/products/21/42.jpg', 1, NULL, 21, 42),
(NULL, 'https://imageshare13.blob.core.windows.net/products/21/item1.jpg', 0, NULL, 21, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/21/item2.jpg', 0, NULL, 21, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/21/item3.jpg', 0, NULL, 21, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/21/item4.jpg', 0, NULL, 21, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/22/43.jpg', 1, NULL, 22, 43),
(NULL, 'https://imageshare13.blob.core.windows.net/products/22/44.jpg', 1, NULL, 22, 44),
(NULL, 'https://imageshare13.blob.core.windows.net/products/22/item1.jpg', 0, NULL, 22, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/22/item2.jpg', 0, NULL, 22, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/22/item3.jpg', 0, NULL, 22, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/22/item4.jpg', 0, NULL, 22, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/23/45.jpeg', 1, NULL, 23, 45),
(NULL, 'https://imageshare13.blob.core.windows.net/products/23/46.jpg', 1, NULL, 23, 46),
(NULL, 'https://imageshare13.blob.core.windows.net/products/23/item1.png', 0, NULL, 23, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/23/item2.png', 0, NULL, 23, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/23/item3.png', 0, NULL, 23, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/23/item4.png', 0, NULL, 23, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/24/47.webp', 1, NULL, 24, 47),
(NULL, 'https://imageshare13.blob.core.windows.net/products/24/48.webp', 1, NULL, 24, 48),
(NULL, 'https://imageshare13.blob.core.windows.net/products/24/item1.jpg', 0, NULL, 24, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/24/item2.jpg', 0, NULL, 24, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/24/item3.jpg', 0, NULL, 24, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/24/item4.jpg', 0, NULL, 24, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/25/49.webp', 1, NULL, 25, 49),
(NULL, 'https://imageshare13.blob.core.windows.net/products/25/50.avif', 1, NULL, 25, 50),
(NULL, 'https://imageshare13.blob.core.windows.net/products/25/item1.jpg', 0, NULL, 25, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/25/item2.jpg', 0, NULL, 25, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/25/item3.jpg', 0, NULL, 25, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/25/item4.jpg', 0, NULL, 25, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/26/51.jpg', 1, NULL, 26, 51),
(NULL, 'https://imageshare13.blob.core.windows.net/products/26/52.jpg', 1, NULL, 26, 52),
(NULL, 'https://imageshare13.blob.core.windows.net/products/26/item1.avif', 0, NULL, 26, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/26/item2.avif', 0, NULL, 26, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/26/item3.avif', 0, NULL, 26, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/26/item4.avif', 0, NULL, 26, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/27/53.webp', 1, NULL, 27, 53),
(NULL, 'https://imageshare13.blob.core.windows.net/products/27/54.webp', 1, NULL, 27, 54),
(NULL, 'https://imageshare13.blob.core.windows.net/products/27/item1.jpg', 0, NULL, 27, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/27/item2.jpg', 0, NULL, 27, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/27/item3.jpg', 0, NULL, 27, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/27/item4.jpg', 0, NULL, 27, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/28/55.jpg', 1, NULL, 28, 55),
(NULL, 'https://imageshare13.blob.core.windows.net/products/28/56.jpg', 1, NULL, 28, 56),
(NULL, 'https://imageshare13.blob.core.windows.net/products/28/item1.jpg', 0, NULL, 28, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/28/item2.jpg', 0, NULL, 28, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/28/item3.jpg', 0, NULL, 28, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/28/item4.jpg', 0, NULL, 28, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/29/57.jpg', 1, NULL, 29, 57),
(NULL, 'https://imageshare13.blob.core.windows.net/products/29/58.jpg', 1, NULL, 29, 58),
(NULL, 'https://imageshare13.blob.core.windows.net/products/29/item1.webp', 0, NULL, 29, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/29/item2.webp', 0, NULL, 29, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/29/item3.webp', 0, NULL, 29, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/29/item4.jpg', 0, NULL, 29, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/30/59.jpg', 1, NULL, 30, 59),
(NULL, 'https://imageshare13.blob.core.windows.net/products/30/60.jpg', 1, NULL, 30, 60),
(NULL, 'https://imageshare13.blob.core.windows.net/products/30/item1.jpg', 0, NULL, 30, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/30/item2.jpg', 0, NULL, 30, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/30/item3.jpg', 0, NULL, 30, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/30/item4.jpg', 0, NULL, 30, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/31/61.webp', 1, NULL, 31, 61),
(NULL, 'https://imageshare13.blob.core.windows.net/products/31/62.webp', 1, NULL, 31, 62),
(NULL, 'https://imageshare13.blob.core.windows.net/products/31/item1.jpg', 0, NULL, 31, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/31/item2.jpg', 0, NULL, 31, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/31/item3.jpg', 0, NULL, 31, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/31/item4.jpg', 0, NULL, 31, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/32/63.png', 1, NULL, 32, 63),
(NULL, 'https://imageshare13.blob.core.windows.net/products/32/64.jpg', 1, NULL, 32, 64),
(NULL, 'https://imageshare13.blob.core.windows.net/products/32/item1.png', 0, NULL, 32, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/32/item2.png', 0, NULL, 32, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/32/item3.png', 0, NULL, 32, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/32/item4.png', 0, NULL, 32, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/33/65.png', 1, NULL, 33, 65),
(NULL, 'https://imageshare13.blob.core.windows.net/products/33/66.png', 1, NULL, 33, 66),
(NULL, 'https://imageshare13.blob.core.windows.net/products/33/item1.jpg', 0, NULL, 33, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/33/item2.jpg', 0, NULL, 33, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/33/item3.jpg', 0, NULL, 33, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/33/item4.jpg', 0, NULL, 33, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/34/67.webp', 1, NULL, 34, 67),
(NULL, 'https://imageshare13.blob.core.windows.net/products/34/68.webp', 1, NULL, 34, 68),
(NULL, 'https://imageshare13.blob.core.windows.net/products/34/item1.jpg', 0, NULL, 34, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/34/item2.jpg', 0, NULL, 34, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/34/item3.jpg', 0, NULL, 34, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/34/item4.jpg', 0, NULL, 34, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/35/69.png', 1, NULL, 35, 69),
(NULL, 'https://imageshare13.blob.core.windows.net/products/35/70.jpg', 1, NULL, 35, 70),
(NULL, 'https://imageshare13.blob.core.windows.net/products/35/item1.jpg', 0, NULL, 35, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/35/item2.jpg', 0, NULL, 35, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/35/item3.jpg', 0, NULL, 35, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/35/item4.jpg', 0, NULL, 35, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/36/71.jpg', 1, NULL, 36, 71),
(NULL, 'https://imageshare13.blob.core.windows.net/products/36/72.jpg', 1, NULL, 36, 72),
(NULL, 'https://imageshare13.blob.core.windows.net/products/36/item1.jpg', 0, NULL, 36, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/36/item2.jpg', 0, NULL, 36, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/36/item3.jpg', 0, NULL, 36, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/36/item4.jpg', 0, NULL, 36, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/37/73.jpg', 1, NULL, 37, 73),
(NULL, 'https://imageshare13.blob.core.windows.net/products/37/74.jpg', 1, NULL, 37, 74),
(NULL, 'https://imageshare13.blob.core.windows.net/products/37/item1.jpg', 0, NULL, 37, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/37/item2.jpg', 0, NULL, 37, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/37/item3.jpg', 0, NULL, 37, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/37/item4.jpg', 0, NULL, 37, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/38/75.webp', 1, NULL, 38, 75),
(NULL, 'https://imageshare13.blob.core.windows.net/products/38/76.jpeg', 1, NULL, 38, 76),
(NULL, 'https://imageshare13.blob.core.windows.net/products/38/item1.jpg', 0, NULL, 38, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/38/item2.jpg', 0, NULL, 38, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/38/item3.jpg', 0, NULL, 38, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/38/item4.jpg', 0, NULL, 38, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/39/77.jpg', 1, NULL, 39, 77),
(NULL, 'https://imageshare13.blob.core.windows.net/products/39/78.jpg', 1, NULL, 39, 78),
(NULL, 'https://imageshare13.blob.core.windows.net/products/39/item1.avif', 0, NULL, 39, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/39/item2.avif', 0, NULL, 39, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/39/item3.avif', 0, NULL, 39, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/39/item4.avif', 0, NULL, 39, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/40/79.jpg', 1, NULL, 40, 79),
(NULL, 'https://imageshare13.blob.core.windows.net/products/40/80.jpg', 1, NULL, 40, 80),
(NULL, 'https://imageshare13.blob.core.windows.net/products/40/item1.jpg', 0, NULL, 40, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/40/item2.jpg', 0, NULL, 40, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/40/item3.jpg', 0, NULL, 40, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/40/item4.jpg', 0, NULL, 40, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/41/81.avif', 1, NULL, 41, 81),
(NULL, 'https://imageshare13.blob.core.windows.net/products/41/82.avif', 1, NULL, 41, 82),
(NULL, 'https://imageshare13.blob.core.windows.net/products/41/item1.jpg', 0, NULL, 41, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/41/item2.webp', 0, NULL, 41, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/41/item3.webp', 0, NULL, 41, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/41/item4.jpg', 0, NULL, 41, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/42/83.png', 1, NULL, 42, 83),
(NULL, 'https://imageshare13.blob.core.windows.net/products/42/84.jpg', 1, NULL, 42, 84),
(NULL, 'https://imageshare13.blob.core.windows.net/products/42/item1.jpg', 0, NULL, 42, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/42/item2.webp', 0, NULL, 42, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/42/item3.webp', 0, NULL, 42, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/42/item4.jpg', 0, NULL, 42, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/43/85.jpg', 1, NULL, 43, 85),
(NULL, 'https://imageshare13.blob.core.windows.net/products/43/86.jpg', 1, NULL, 43, 86),
(NULL, 'https://imageshare13.blob.core.windows.net/products/43/item1.jpg', 0, NULL, 43, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/43/item2.jpg', 0, NULL, 43, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/43/item3.jpg', 0, NULL, 43, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/43/item4.jpg', 0, NULL, 43, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/44/87.webp', 1, NULL, 44, 87),
(NULL, 'https://imageshare13.blob.core.windows.net/products/44/88.jpg', 1, NULL, 44, 88),
(NULL, 'https://imageshare13.blob.core.windows.net/products/44/item1.jpg', 0, NULL, 44, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/44/item2.jpg', 0, NULL, 44, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/44/item3.jpg', 0, NULL, 44, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/44/item4.jpg', 0, NULL, 44, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/45/89.webp', 1, NULL, 45, 89),
(NULL, 'https://imageshare13.blob.core.windows.net/products/45/90.png', 1, NULL, 45, 90),
(NULL, 'https://imageshare13.blob.core.windows.net/products/45/item1.jpg', 0, NULL, 45, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/45/item2.jpg', 0, NULL, 45, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/45/item3.jpg', 0, NULL, 45, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/45/item4.jpg', 0, NULL, 45, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/46/91.webp', 1, NULL, 46, 91),
(NULL, 'https://imageshare13.blob.core.windows.net/products/46/92.png', 1, NULL, 46, 92),
(NULL, 'https://imageshare13.blob.core.windows.net/products/46/item1.avif', 0, NULL, 46, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/46/item2.avif', 0, NULL, 46, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/46/item3.avif', 0, NULL, 46, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/46/item4.avif', 0, NULL, 46, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/47/93.jpg', 1, NULL, 47, 93),
(NULL, 'https://imageshare13.blob.core.windows.net/products/47/94.jpg', 1, NULL, 47, 94),
(NULL, 'https://imageshare13.blob.core.windows.net/products/47/item1.jpg', 0, NULL, 47, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/47/item2.jpg', 0, NULL, 47, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/47/item3.jpg', 0, NULL, 47, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/47/item4.jpg', 0, NULL, 47, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/48/95.png', 1, NULL, 48, 95),
(NULL, 'https://imageshare13.blob.core.windows.net/products/48/96.jpg', 1, NULL, 48, 96),
(NULL, 'https://imageshare13.blob.core.windows.net/products/48/item1.jpg', 0, NULL, 48, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/48/item2.jpg', 0, NULL, 48, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/48/item3.jpg', 0, NULL, 48, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/48/item4.jpg', 0, NULL, 48, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/49/97.jpg', 1, NULL, 49, 97),
(NULL, 'https://imageshare13.blob.core.windows.net/products/49/98.jpg', 1, NULL, 49, 98),
(NULL, 'https://imageshare13.blob.core.windows.net/products/49/item1.jpg', 0, NULL, 49, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/49/item2.webp', 0, NULL, 49, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/49/item3.webp', 0, NULL, 49, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/49/item4.webp', 0, NULL, 49, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/50/99.jpg', 1, NULL, 50, 99),
(NULL, 'https://imageshare13.blob.core.windows.net/products/50/100.jpg', 1, NULL, 50, 100),
(NULL, 'https://imageshare13.blob.core.windows.net/products/50/item1.jpg', 0, NULL, 50, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/50/item2.jpg', 0, NULL, 50, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/50/item3.jpg', 0, NULL, 50, NULL),
(NULL, 'https://imageshare13.blob.core.windows.net/products/50/item4.jpg', 0, NULL, 50, NULL);


INSERT INTO coupons ( code, name, discount_type, discount_value, min_order_value, max_discount_amount, usage_limit, used_count, usage_limit_per_user, start_date, end_date, is_active) VALUES
('WELCOME1T', N'Giảm 1 triệu cho khách mới', 'fixed_amount', 1000000, 15000000, NULL, 1000, 150, 1, '2026-01-01 00:00:00', '2026-12-31 23:59:59', 1),
( 'SUMMER5', N'Giảm 5% đón hè rực rỡ', 'percentage', 5, 20000000, 2000000, 500, 200, 1, '2026-05-01 00:00:00', '2026-08-31 23:59:59', 1),
( 'FLASHVIP', N'Flash Sale giảm 2 triệu', 'fixed_amount', 2000000, 40000000, NULL, 100, 99, 1, '2026-06-01 00:00:00', '2026-06-15 23:59:59', 1),
('HONDAFAN', N'Giảm 3% riêng xe Honda', 'percentage', 3, 20000000, 1500000, 300, 120, 2, '2026-04-01 00:00:00', '2026-07-31 23:59:59', 1),
('PKL10M', N'Giảm 10 triệu cho siêu xe PKL', 'fixed_amount', 10000000, 300000000, NULL, 50, 5, 1, '2026-01-01 00:00:00', '2026-12-31 23:59:59', 1),
('TET2026', N'Lì xì đầu năm 2026', 'fixed_amount', 500000, 15000000, NULL, 2000, 1950, 1, '2026-01-01 00:00:00', '2026-02-28 23:59:59', 0), -- Đã hết hạn
('EVO500', N'Giảm 500K xe điện xanh', 'fixed_amount', 500000, 20000000, NULL, 500, 80, 1, '2026-03-01 00:00:00', '2026-09-30 23:59:59', 1),
('GA1M', N'Giảm 1 triệu xe tay ga nữ', 'fixed_amount', 1000000, 30000000, NULL, 300, 250, 1, '2026-03-08 00:00:00', '2026-10-20 23:59:59', 1),
('LUXURY10', N'Giảm 10% dòng xe cao cấp', 'percentage', 10, 80000000, 5000000, 100, 20, 1, '2026-06-01 00:00:00', '2026-07-31 23:59:59', 1),
('STUDENT26', N'Hỗ trợ sinh viên tựu trường', 'fixed_amount', 800000, 15000000, NULL, 1000, 450, 1, '2026-08-01 00:00:00', '2026-10-31 23:59:59', 1);

INSERT INTO promotions (name, discount_type, discount_value, start_date, end_date, is_active) VALUES
(N'Xả hàng tồn kho cuối năm', 'percentage', 5, '2025-12-01 00:00:00', '2025-12-31 23:59:59', 0), -- Đã hết hạn
(N'Khuyến mãi Hè Sôi Động 2026', 'fixed_amount', 1000000, '2026-05-01 00:00:00', '2026-08-31 23:59:59', 1),
(N'Tháng Vàng Honda - Lái xe thả ga', 'percentage', 3, '2026-06-01 00:00:00', '2026-06-30 23:59:59', 1),
(N'Tuần lễ tay côn Yamaha', 'fixed_amount', 1500000, '2026-06-10 00:00:00', '2026-06-20 23:59:59', 1),
(N'Chuyển đổi xanh cùng VinFast', 'percentage', 10, '2026-01-01 00:00:00', '2026-12-31 23:59:59', 1),
(N'Mừng sinh nhật hệ thống', 'fixed_amount', 2000000, '2026-07-01 00:00:00', '2026-07-15 23:59:59', 1),
(N'Bùng nổ đam mê PKL Kawasaki', 'fixed_amount', 5000000, '2026-05-15 00:00:00', '2026-08-15 23:59:59', 1),
(N'Ngày hội Vespa sành điệu', 'percentage', 5, '2026-06-01 00:00:00', '2026-07-31 23:59:59', 1),
(N'Clearance Sale xe côn tay Suzuki', 'fixed_amount', 3000000, '2026-04-01 00:00:00', '2026-06-30 23:59:59', 1),
(N'Sale sốc giá hời xe số', 'fixed_amount', 500000, '2026-06-05 00:00:00', '2026-06-25 23:59:59', 1);

INSERT INTO ProductPromotions (promotion_id, product_id) VALUES
(2, 3),   
(2, 5),   
(2, 8),   
(3, 1),   
(3, 7),   
(3, 12),  
(4, 6),   
(4, 17), 
(4, 28), 
(5, 34), 
(5, 35), 
(5, 36),  
(6, 24), 
(6, 29),  
(6, 49), 
(7, 16),  
(7, 37),  
(7, 47), 
(8, 11),  
(8, 41), 
(9, 19), 
(10, 2), 
(10, 4); 

INSERT INTO CancellationReasons (code, content, display_order)
VALUES 
    ('CHANGE_MIND', N'Tôi muốn đổi sang dòng xe / màu xe khác', 1),
    ('FOUND_BETTER_PRICE', N'Tôi tìm thấy nơi khác có giá tốt hơn', 2),
    ('HIGH_SHIPPING', N'Phí vận chuyển cao / Thời gian chờ giao xe quá lâu', 3),
    ('FINANCE_ISSUE', N'Tôi gặp khó khăn về thủ tục thanh toán / trả góp', 4),
    ('JUST_CANCEL', N'Tôi chỉ đổi ý, không muốn mua nữa', 5),
    ('OTHER', N'Lý do khác', 99);