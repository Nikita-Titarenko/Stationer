USE [master]
GO
/****** Object:  Database [Stationer ]    Script Date: 13.02.2025 19:14:33 ******/
CREATE DATABASE [Stationer ]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Stationer', FILENAME = N'D:\Для извлечения\Кек\Stationer .mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Stationer _log', FILENAME = N'D:\Для извлечения\Кек\Stationer _log.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [Stationer ] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Stationer ].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Stationer ] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Stationer ] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Stationer ] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Stationer ] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Stationer ] SET ARITHABORT OFF 
GO
ALTER DATABASE [Stationer ] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Stationer ] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Stationer ] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Stationer ] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Stationer ] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Stationer ] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Stationer ] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Stationer ] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Stationer ] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Stationer ] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Stationer ] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Stationer ] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Stationer ] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Stationer ] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Stationer ] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Stationer ] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Stationer ] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Stationer ] SET RECOVERY FULL 
GO
ALTER DATABASE [Stationer ] SET  MULTI_USER 
GO
ALTER DATABASE [Stationer ] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Stationer ] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Stationer ] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Stationer ] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Stationer ] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [Stationer ] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'Stationer ', N'ON'
GO
ALTER DATABASE [Stationer ] SET QUERY_STORE = ON
GO
ALTER DATABASE [Stationer ] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [Stationer ]
GO
/****** Object:  UserDefinedFunction [dbo].[CartSum]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[CartSum](
@cart_id int
)
RETURNS DECIMAL(10, 2)
AS
BEGIN
DECLARE @sum DECIMAL(10, 2)
SELECT @sum = SUM(dbo.PriceWithDiscount(p.price, p.discount_rate) * pc.quantity) 
FROM product p 
INNER JOIN product_cart pc 
ON p.article = pc.article
WHERE pc.cart_id = @cart_id
RETURN @sum
END
GO
/****** Object:  UserDefinedFunction [dbo].[CheckAvailability]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[CheckAvailability](
    @stock_quantity int
)
RETURNS varchar(50)
AS
BEGIN
    DECLARE @availability varchar(50)

    SELECT @availability = 
        CASE 
            WHEN @stock_quantity > 0 THEN 
                CONCAT('Є в наявності (', CAST(@stock_quantity AS varchar), ')')
            ELSE 
                'Немає в наявності' 
        END

    RETURN @availability
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetCartIP]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetCartIP]
(
@ip_address varchar(15)
)
RETURNS INT
AS
BEGIN
DECLARE @cart_id INT;
	SELECT @cart_id = cart_id FROM cart WHERE ip_address = @ip_address
	RETURN @cart_id;
END
GO
/****** Object:  UserDefinedFunction [dbo].[MonthlySales]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[MonthlySales](
@article varchar(6)
)
RETURNS int
AS
BEGIN
DECLARE @monthly_sales int
SELECT @monthly_sales = SUM(pro.quantity) FROM product_order pro INNER JOIN [order] o ON pro.order_number = o.order_number WHERE @article = pro.article
AND DATEADD(MONTH, 1, o.date_time) > GETDATE()
RETURN @monthly_sales
END
GO
/****** Object:  UserDefinedFunction [dbo].[PriceWithDiscount]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[PriceWithDiscount](
@price int,
@discount_rate int
)
RETURNS DECIMAL(10, 2)
AS
BEGIN
	RETURN CAST(ROUND(@price * ((100.0 - ISNULL(@discount_rate, 0)) / 100.0), 2) AS DECIMAL(10, 2));
END
GO
/****** Object:  Table [dbo].[cart]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[cart](
	[cart_id] [int] IDENTITY(1,1) NOT NULL,
	[ip_address] [varchar](15) NOT NULL,
	[creation_date] [date] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[cart_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[category]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[category](
	[category_id] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](50) NULL,
	[description] [varchar](1000) NULL,
PRIMARY KEY CLUSTERED 
(
	[category_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[comment]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[comment](
	[comment_id] [int] IDENTITY(1,1) NOT NULL,
	[content] [varchar](500) NOT NULL,
	[rating] [int] NOT NULL,
	[advantages] [varchar](500) NULL,
	[disadvantages] [varchar](500) NULL,
	[article] [varchar](6) NULL,
	[order_number] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[comment_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[article] ASC,
	[order_number] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[order]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[order](
	[order_number] [int] IDENTITY(1,1) NOT NULL,
	[payment_method] [varchar](50) NOT NULL,
	[delivery_type] [varchar](50) NULL,
	[delivery_method] [varchar](50) NOT NULL,
	[order_comment] [varchar](500) NULL,
	[email] [varchar](50) NULL,
	[region] [varchar](50) NULL,
	[city] [varchar](50) NULL,
	[street] [varchar](50) NULL,
	[apartment_number] [int] NULL,
	[house_number] [int] NULL,
	[full_name] [varchar](50) NOT NULL,
	[phone] [varchar](15) NULL,
	[date_time] [datetime] NOT NULL,
	[branch_number] [int] NULL,
	[order_status] [varchar](50) NOT NULL,
	[user_id] [int] NULL,
 CONSTRAINT [PK__order__46596229EAE8F7CC] PRIMARY KEY CLUSTERED 
(
	[order_number] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[product]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[product](
	[article] [varchar](6) NOT NULL,
	[name] [varchar](200) NULL,
	[price] [float] NULL,
	[brand] [varchar](50) NULL,
	[creation_date] [date] NULL,
	[stock_quantity] [int] NULL,
	[description] [varchar](500) NULL,
	[initial_quantity] [int] NULL,
	[discount_rate] [int] NULL,
	[subcategory_id] [int] NULL,
 CONSTRAINT [PK_products] PRIMARY KEY CLUSTERED 
(
	[article] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[product_cart]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[product_cart](
	[quantity] [int] NOT NULL,
	[creation_date] [date] NOT NULL,
	[article] [varchar](6) NOT NULL,
	[cart_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[article] ASC,
	[cart_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[product_order]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[product_order](
	[quantity] [int] NOT NULL,
	[discount_percentage] [int] NULL,
	[article] [varchar](6) NOT NULL,
	[order_number] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[article] ASC,
	[order_number] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[product_subcategory_attribute]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[product_subcategory_attribute](
	[value] [varchar](50) NULL,
	[article] [varchar](6) NOT NULL,
	[subcategory_attribute_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[article] ASC,
	[subcategory_attribute_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[property]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[property](
	[property_id] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[property_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[subcategory]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[subcategory](
	[subcategory_id] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](50) NULL,
	[description] [varchar](1000) NULL,
	[category_id] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[subcategory_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[subcategory_attribute]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[subcategory_attribute](
	[subcategory_attribute_id] [int] IDENTITY(1,1) NOT NULL,
	[subcategory_id] [int] NULL,
	[property_id] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[subcategory_attribute_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[user]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[user](
	[user_id] [int] IDENTITY(1,1) NOT NULL,
	[full_name] [varchar](50) NULL,
	[phone] [varchar](15) NULL,
	[gender] [varchar](7) NULL,
	[birthday_date] [date] NULL,
	[username] [varchar](20) NOT NULL,
	[password] [varchar](20) NOT NULL,
	[creation_date] [date] NOT NULL,
	[payment_method] [varchar](50) NULL,
	[delivery_method] [varchar](50) NULL,
	[cart_id] [int] NOT NULL,
	[email] [varchar](50) NULL,
 CONSTRAINT [PK__user__B9BE370F9D85CE87] PRIMARY KEY CLUSTERED 
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ__user__2EF52A26F25A6E65] UNIQUE NONCLUSTERED 
(
	[cart_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[order] ADD  CONSTRAINT [DF_order_order_status]  DEFAULT ('Обробляється') FOR [order_status]
GO
ALTER TABLE [dbo].[comment]  WITH CHECK ADD FOREIGN KEY([article], [order_number])
REFERENCES [dbo].[product_order] ([article], [order_number])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[order]  WITH CHECK ADD  CONSTRAINT [user_id] FOREIGN KEY([user_id])
REFERENCES [dbo].[user] ([user_id])
GO
ALTER TABLE [dbo].[order] CHECK CONSTRAINT [user_id]
GO
ALTER TABLE [dbo].[product]  WITH CHECK ADD  CONSTRAINT [FK__product__subcate__40058253] FOREIGN KEY([subcategory_id])
REFERENCES [dbo].[subcategory] ([subcategory_id])
ON UPDATE CASCADE
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[product] CHECK CONSTRAINT [FK__product__subcate__40058253]
GO
ALTER TABLE [dbo].[product_cart]  WITH CHECK ADD FOREIGN KEY([cart_id])
REFERENCES [dbo].[cart] ([cart_id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[product_cart]  WITH CHECK ADD  CONSTRAINT [FK__product_c__produ__51300E55] FOREIGN KEY([article])
REFERENCES [dbo].[product] ([article])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[product_cart] CHECK CONSTRAINT [FK__product_c__produ__51300E55]
GO
ALTER TABLE [dbo].[product_order]  WITH CHECK ADD  CONSTRAINT [FK__product_o__order__3493CFA7] FOREIGN KEY([order_number])
REFERENCES [dbo].[order] ([order_number])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[product_order] CHECK CONSTRAINT [FK__product_o__order__3493CFA7]
GO
ALTER TABLE [dbo].[product_order]  WITH CHECK ADD  CONSTRAINT [FK__product_o__produ__339FAB6E] FOREIGN KEY([article])
REFERENCES [dbo].[product] ([article])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[product_order] CHECK CONSTRAINT [FK__product_o__produ__339FAB6E]
GO
ALTER TABLE [dbo].[product_subcategory_attribute]  WITH CHECK ADD  CONSTRAINT [FK__product_s__artic__671F4F74] FOREIGN KEY([article])
REFERENCES [dbo].[product] ([article])
GO
ALTER TABLE [dbo].[product_subcategory_attribute] CHECK CONSTRAINT [FK__product_s__artic__671F4F74]
GO
ALTER TABLE [dbo].[product_subcategory_attribute]  WITH CHECK ADD FOREIGN KEY([subcategory_attribute_id])
REFERENCES [dbo].[subcategory_attribute] ([subcategory_attribute_id])
GO
ALTER TABLE [dbo].[subcategory]  WITH CHECK ADD FOREIGN KEY([category_id])
REFERENCES [dbo].[category] ([category_id])
ON UPDATE CASCADE
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[subcategory_attribute]  WITH CHECK ADD FOREIGN KEY([property_id])
REFERENCES [dbo].[property] ([property_id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[subcategory_attribute]  WITH CHECK ADD FOREIGN KEY([subcategory_id])
REFERENCES [dbo].[subcategory] ([subcategory_id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[user]  WITH CHECK ADD  CONSTRAINT [FK__user__cart_id__56E8E7AB] FOREIGN KEY([cart_id])
REFERENCES [dbo].[cart] ([cart_id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[user] CHECK CONSTRAINT [FK__user__cart_id__56E8E7AB]
GO
ALTER TABLE [dbo].[product]  WITH CHECK ADD  CONSTRAINT [discount_rate] CHECK  (([discount_rate]>=(1) AND [discount_rate]<=(50)))
GO
ALTER TABLE [dbo].[product] CHECK CONSTRAINT [discount_rate]
GO
ALTER TABLE [dbo].[product]  WITH CHECK ADD  CONSTRAINT [initial_quantity] CHECK  (([initial_quantity]>(0)))
GO
ALTER TABLE [dbo].[product] CHECK CONSTRAINT [initial_quantity]
GO
ALTER TABLE [dbo].[user]  WITH CHECK ADD  CONSTRAINT [CK__user__gender__55F4C372] CHECK  (([gender]='Жінка' OR [gender]='Чоловік'))
GO
ALTER TABLE [dbo].[user] CHECK CONSTRAINT [CK__user__gender__55F4C372]
GO
/****** Object:  StoredProcedure [dbo].[AddInCart]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AddInCart]

@cart_id int,

@article varchar(6),

@quantity int

AS
BEGIN
IF EXISTS(SELECT 1 FROM product pr LEFT JOIN product_cart prc ON pr.article = prc.article WHERE pr.stock_quantity < @quantity + ISNULL(prc.quantity, 0) AND pr.article = @article)
BEGIN
SELECT pr.stock_quantity FROM product pr WHERE pr.article = @article;
END
ELSE
BEGIN
IF NOT EXISTS (SELECT 1 FROM product_cart WHERE article = @article AND cart_id = @cart_id)

BEGIN

	INSERT INTO product_cart

	(cart_id, article, quantity, creation_date)

	VALUES (@cart_id, @article, @quantity, GETDATE())

END
ELSE
BEGIN
	UPDATE product_cart SET quantity += @quantity WHERE article = @article AND cart_id = @cart_id
END
SELECT -1;
END
END
GO
/****** Object:  StoredProcedure [dbo].[CreateCart]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreateCart]
@ip_address varchar(15)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO cart (ip_address, creation_date) VALUES (@ip_address, GETDATE());
    SELECT SCOPE_IDENTITY();
END
GO
/****** Object:  StoredProcedure [dbo].[Login]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Login]
@username varchar(50),
@password varchar(50),
@cart_id int,
@ip_address varchar(15)
AS
BEGIN
	IF EXISTS (SELECT 1 FROM [user] WHERE username = @username AND password = @password)
	BEGIN
		SELECT full_name, user_id, cart_id FROM [user] WHERE username = @username AND password = @password
		UPDATE cart SET ip_address = @ip_address WHERE @username IN (SELECT username FROM [user] WHERE username = @username)
		IF @cart_id IS NOT NULL
		BEGIN
		DELETE FROM cart WHERE cart_id = @cart_id
		END
	END
END
GO
/****** Object:  StoredProcedure [dbo].[PlaceOrder]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PlaceOrder]
@payment_method varchar(50),
@delivery_type varchar(50),
@delivery_method varchar(50),
@order_comment varchar(500),
@email varchar(50),
@region varchar(50),
@city varchar(50),
@street varchar(50),
@apartment_number int,
@house_number int,
@full_name varchar(50),
@phone varchar(15),
@branch_number int,
@user_id int,
@cart_id int
AS
BEGIN
IF EXISTS(SELECT 1 FROM product pr JOIN product_cart prc ON pr.article = prc.article WHERE pr.stock_quantity < prc.quantity AND prc.cart_id = @cart_id)
BEGIN
SELECT -1;
END
ELSE
BEGIN
	INSERT INTO [order]
	(user_id, payment_method, delivery_method, date_time, delivery_type, order_comment, email, region, city, street, apartment_number, house_number, full_name, phone, branch_number)
	VALUES (@user_id, @payment_method, @delivery_method, GETDATE(), @delivery_type, @order_comment, @email, @region, @city, @street, @apartment_number, @house_number, @full_name, @phone, @branch_number)
	DECLARE @order_number int;
	SET @order_number = SCOPE_IDENTITY();

	INSERT INTO product_order (order_number, pc.article, pc.quantity, pc.discount_percentage)
	SELECT @order_number, pc.article, pc.quantity, p.discount_rate
	FROM product_cart pc INNER JOIN product p ON pc.article = p.article
	WHERE @cart_id = pc.cart_id

	UPDATE pr SET pr.stock_quantity -= prc.quantity FROM product AS pr JOIN product_cart prc ON pr.article = prc.article WHERE prc.cart_id = @cart_id;

	DELETE FROM product_cart WHERE cart_id = @cart_id;

	SELECT @order_number;
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Register]    Script Date: 13.02.2025 19:14:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Register]
@full_name varchar(50),
@phone varchar(15),
@email varchar(50),
@username varchar(50),
@password varchar(50),
@cart_id int
AS
IF @username NOT IN (SELECT username FROM [user])
BEGIN
    INSERT INTO [user]
                         (full_name, phone, email, username, password, creation_date, cart_id)
    VALUES        (@full_name, @phone, @email, @username, @password, GETDATE(), @cart_id)
    SELECT SCOPE_IDENTITY() AS NewID
END
GO
USE [master]
GO
ALTER DATABASE [Stationer ] SET  READ_WRITE 
GO
