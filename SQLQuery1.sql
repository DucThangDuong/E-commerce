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

-- ==========================================
-- 3. Bảng Sản phẩm (Products)
-- ==========================================
CREATE TABLE Products (
    product_id INT IDENTITY(1,1) PRIMARY KEY,
    category_id INT NOT NULL,
    SKU VARCHAR(50) NOT NULL UNIQUE,
    name NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
    base_price DECIMAL(18, 2) NOT NULL,
    FOREIGN KEY (category_id) REFERENCES Categories(category_id)
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