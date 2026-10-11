USE [master]
GO
/****** Objeto: Database [DBBELISA] Fecha de script: 10/10/2026 8:27:53 PM ******/
CREATE DATABASE [DBBELISA]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'DBBELISA', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.MSSQLSERVER01\MSSQL\DATA\DBBELISA.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'DBBELISA_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.MSSQLSERVER01\MSSQL\DATA\DBBELISA_log.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [DBBELISA] SET COMPATIBILITY_LEVEL = 170
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [DBBELISA].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [DBBELISA] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DBBELISA] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DBBELISA] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DBBELISA] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DBBELISA] SET ARITHABORT OFF 
GO
ALTER DATABASE [DBBELISA] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [DBBELISA] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DBBELISA] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DBBELISA] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DBBELISA] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DBBELISA] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DBBELISA] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DBBELISA] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DBBELISA] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DBBELISA] SET  ENABLE_BROKER 
GO
ALTER DATABASE [DBBELISA] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DBBELISA] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DBBELISA] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DBBELISA] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DBBELISA] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DBBELISA] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [DBBELISA] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [DBBELISA] SET RECOVERY FULL 
GO
ALTER DATABASE [DBBELISA] SET  MULTI_USER 
GO
ALTER DATABASE [DBBELISA] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DBBELISA] SET DB_CHAINING OFF 
GO
ALTER DATABASE [DBBELISA] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [DBBELISA] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [DBBELISA] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [DBBELISA] SET OPTIMIZED_LOCKING = OFF 
GO
ALTER DATABASE [DBBELISA] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'DBBELISA', N'ON'
GO
ALTER DATABASE [DBBELISA] SET QUERY_STORE = ON
GO
ALTER DATABASE [DBBELISA] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [DBBELISA]
GO
/****** Objeto: UserDefinedTableType [dbo].[EDetalle_Compra] Fecha de script: 10/10/2026 8:27:53 PM ******/
CREATE TYPE [dbo].[EDetalle_Compra] AS TABLE(
	[IdProducto] [int] NOT NULL,
	[PrecioCompra] [decimal](10, 2) NOT NULL,
	[PrecioVenta] [decimal](10, 2) NOT NULL,
	[Cantidad] [int] NOT NULL
)
GO
/****** Objeto: UserDefinedTableType [dbo].[EDetalle_Venta] Fecha de script: 10/10/2026 8:27:54 PM ******/
CREATE TYPE [dbo].[EDetalle_Venta] AS TABLE(
	[IdProducto] [int] NOT NULL,
	[PrecioVenta] [decimal](10, 2) NOT NULL,
	[Cantidad] [int] NOT NULL
)
GO
/****** Objeto: UserDefinedTableType [dbo].[EPago_Venta] Fecha de script: 10/10/2026 8:27:54 PM ******/
CREATE TYPE [dbo].[EPago_Venta] AS TABLE(
	[MetodoPago] [varchar](30) NOT NULL,
	[Monto] [decimal](10, 2) NOT NULL,
	[ReferenciaPago] [varchar](50) NULL
)
GO
/****** Objeto: Table [dbo].[AJUSTE_INVENTARIO] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AJUSTE_INVENTARIO](
	[IdAjuste] [int] IDENTITY(1,1) NOT NULL,
	[IdProducto] [int] NOT NULL,
	[IdUsuario] [int] NOT NULL,
	[StockAnterior] [int] NOT NULL,
	[StockNuevo] [int] NOT NULL,
	[Diferencia]  AS ([StockNuevo]-[StockAnterior]) PERSISTED,
	[Motivo] [varchar](30) NOT NULL,
	[Nota] [varchar](200) NULL,
	[FechaRegistro] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdAjuste] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[CATEGORIA] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CATEGORIA](
	[IdCategoria] [int] IDENTITY(1,1) NOT NULL,
	[Descripcion] [varchar](100) NOT NULL,
	[Estado] [bit] NOT NULL,
	[FechaRegistro] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdCategoria] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Descripcion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[CLIENTE] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CLIENTE](
	[IdCliente] [int] IDENTITY(1,1) NOT NULL,
	[Documento] [varchar](50) NULL,
	[NombreCompleto] [varchar](100) NOT NULL,
	[Correo] [varchar](100) NULL,
	[Telefono] [varchar](50) NULL,
	[Estado] [bit] NOT NULL,
	[FechaRegistro] [datetime] NOT NULL,
	[CumpleDia] [tinyint] NULL,
	[CumpleMes] [tinyint] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdCliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[COMPRA] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMPRA](
	[IdCompra] [int] IDENTITY(1,1) NOT NULL,
	[IdUsuario] [int] NOT NULL,
	[IdProveedor] [int] NOT NULL,
	[TipoDocumento] [varchar](50) NOT NULL,
	[NumeroDocumento] [varchar](50) NOT NULL,
	[MontoTotal] [decimal](10, 2) NOT NULL,
	[FechaRegistro] [datetime] NOT NULL,
	[MetodoPago] [varchar](30) NOT NULL,
	[ReferenciaPago] [varchar](50) NULL,
	[Anulada] [bit] NOT NULL,
	[FechaAnulacion] [datetime] NULL,
	[IdUsuarioAnula] [int] NULL,
	[MotivoAnulacion] [varchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[IdCompra] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[CONTADOR] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CONTADOR](
	[Nombre] [varchar](20) NOT NULL,
	[Valor] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[DETALLE_COMPRA] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DETALLE_COMPRA](
	[IdDetalleCompra] [int] IDENTITY(1,1) NOT NULL,
	[IdCompra] [int] NOT NULL,
	[IdProducto] [int] NOT NULL,
	[PrecioCompra] [decimal](10, 2) NOT NULL,
	[PrecioVenta] [decimal](10, 2) NOT NULL,
	[Cantidad] [int] NOT NULL,
	[MontoTotal] [decimal](10, 2) NOT NULL,
	[FechaRegistro] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdDetalleCompra] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[DETALLE_VENTA] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DETALLE_VENTA](
	[IdDetalleVenta] [int] IDENTITY(1,1) NOT NULL,
	[IdVenta] [int] NOT NULL,
	[IdProducto] [int] NOT NULL,
	[PrecioVenta] [decimal](10, 2) NOT NULL,
	[Cantidad] [int] NOT NULL,
	[SubTotal] [decimal](10, 2) NOT NULL,
	[FechaRegistro] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdDetalleVenta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[PERMISO] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PERMISO](
	[IdPermiso] [int] IDENTITY(1,1) NOT NULL,
	[IdRol] [int] NOT NULL,
	[NombreMenu] [varchar](100) NOT NULL,
	[FechaRegistro] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdPermiso] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_PERMISO] UNIQUE NONCLUSTERED 
(
	[IdRol] ASC,
	[NombreMenu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[PRODUCTO] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PRODUCTO](
	[IdProducto] [int] IDENTITY(1,1) NOT NULL,
	[Codigo] [varchar](100) NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
	[IdCategoria] [int] NOT NULL,
	[Stock] [int] NOT NULL,
	[PrecioCompra] [decimal](10, 2) NOT NULL,
	[PrecioVenta] [decimal](10, 2) NOT NULL,
	[Descripcion] [varchar](200) NULL,
	[Estado] [bit] NOT NULL,
	[FechaRegistro] [datetime] NOT NULL,
	[StockMinimo] [int] NOT NULL,
	[EsServicio] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdProducto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[PROVEEDOR] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PROVEEDOR](
	[IdProveedor] [int] IDENTITY(1,1) NOT NULL,
	[Documento] [varchar](50) NULL,
	[RazonSocial] [varchar](100) NOT NULL,
	[Correo] [varchar](100) NULL,
	[Telefono] [varchar](50) NULL,
	[Estado] [bit] NOT NULL,
	[FechaRegistro] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdProveedor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[ROL] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ROL](
	[IdRol] [int] IDENTITY(1,1) NOT NULL,
	[Descripcion] [varchar](50) NOT NULL,
	[FechaRegistro] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdRol] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Descripcion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[USUARIO] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[USUARIO](
	[IdUsuario] [int] IDENTITY(1,1) NOT NULL,
	[NameUser] [varchar](50) NOT NULL,
	[NombreCompleto] [varchar](100) NOT NULL,
	[Correo] [varchar](100) NULL,
	[ClaveHash] [varchar](200) NOT NULL,
	[IdRol] [int] NOT NULL,
	[Estado] [bit] NOT NULL,
	[IntentosFallidos] [int] NOT NULL,
	[BloqueadoHasta] [datetime] NULL,
	[FechaRegistro] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[NameUser] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[VENTA] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VENTA](
	[IdVenta] [int] IDENTITY(1,1) NOT NULL,
	[IdUsuario] [int] NOT NULL,
	[IdCliente] [int] NULL,
	[TipoDocumento] [varchar](50) NOT NULL,
	[NumeroDocumento] [varchar](50) NOT NULL,
	[MontoPago] [decimal](10, 2) NOT NULL,
	[MontoCambio] [decimal](10, 2) NOT NULL,
	[MontoTotal] [decimal](10, 2) NOT NULL,
	[FechaRegistro] [datetime] NOT NULL,
	[MetodoPago] [varchar](30) NOT NULL,
	[ReferenciaPago] [varchar](50) NULL,
	[Subtotal] [decimal](10, 2) NOT NULL,
	[DescuentoPorcentaje] [decimal](5, 2) NOT NULL,
	[MontoDescuento] [decimal](10, 2) NOT NULL,
	[Anulada] [bit] NOT NULL,
	[FechaAnulacion] [datetime] NULL,
	[IdUsuarioAnula] [int] NULL,
	[MotivoAnulacion] [varchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[IdVenta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_VENTA_DOC] UNIQUE NONCLUSTERED 
(
	[TipoDocumento] ASC,
	[NumeroDocumento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[VENTA_PAGO] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VENTA_PAGO](
	[IdVentaPago] [int] IDENTITY(1,1) NOT NULL,
	[IdVenta] [int] NOT NULL,
	[MetodoPago] [varchar](30) NOT NULL,
	[Monto] [decimal](10, 2) NOT NULL,
	[ReferenciaPago] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[IdVentaPago] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [UX_CLIENTE_Documento] Fecha de script: 10/10/2026 8:27:54 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_CLIENTE_Documento] ON [dbo].[CLIENTE]
(
	[Documento] ASC
)
WHERE ([Documento] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [UX_COMPRA_Numero] Fecha de script: 10/10/2026 8:27:54 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_COMPRA_Numero] ON [dbo].[COMPRA]
(
	[NumeroDocumento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [UX_PROVEEDOR_Documento] Fecha de script: 10/10/2026 8:27:54 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_PROVEEDOR_Documento] ON [dbo].[PROVEEDOR]
(
	[Documento] ASC
)
WHERE ([Documento] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_VENTA_PAGO_Venta] Fecha de script: 10/10/2026 8:27:54 PM ******/
CREATE NONCLUSTERED INDEX [IX_VENTA_PAGO_Venta] ON [dbo].[VENTA_PAGO]
(
	[IdVenta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AJUSTE_INVENTARIO] ADD  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[CATEGORIA] ADD  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[CATEGORIA] ADD  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[CLIENTE] ADD  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[CLIENTE] ADD  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[COMPRA] ADD  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[COMPRA] ADD  CONSTRAINT [DF_COMPRA_Metodo]  DEFAULT ('Efectivo') FOR [MetodoPago]
GO
ALTER TABLE [dbo].[COMPRA] ADD  CONSTRAINT [DF_COMPRA_Anulada]  DEFAULT ((0)) FOR [Anulada]
GO
ALTER TABLE [dbo].[DETALLE_COMPRA] ADD  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[DETALLE_VENTA] ADD  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[PERMISO] ADD  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[PRODUCTO] ADD  DEFAULT ((0)) FOR [Stock]
GO
ALTER TABLE [dbo].[PRODUCTO] ADD  DEFAULT ((0)) FOR [PrecioCompra]
GO
ALTER TABLE [dbo].[PRODUCTO] ADD  DEFAULT ((0)) FOR [PrecioVenta]
GO
ALTER TABLE [dbo].[PRODUCTO] ADD  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[PRODUCTO] ADD  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[PRODUCTO] ADD  CONSTRAINT [DF_PRODUCTO_StockMinimo]  DEFAULT ((5)) FOR [StockMinimo]
GO
ALTER TABLE [dbo].[PRODUCTO] ADD  CONSTRAINT [DF_PRODUCTO_EsServicio]  DEFAULT ((0)) FOR [EsServicio]
GO
ALTER TABLE [dbo].[PROVEEDOR] ADD  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[PROVEEDOR] ADD  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[ROL] ADD  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[USUARIO] ADD  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[USUARIO] ADD  DEFAULT ((0)) FOR [IntentosFallidos]
GO
ALTER TABLE [dbo].[USUARIO] ADD  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[VENTA] ADD  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[VENTA] ADD  CONSTRAINT [DF_VENTA_Metodo]  DEFAULT ('Efectivo') FOR [MetodoPago]
GO
ALTER TABLE [dbo].[VENTA] ADD  CONSTRAINT [DF_VENTA_Subtotal]  DEFAULT ((0)) FOR [Subtotal]
GO
ALTER TABLE [dbo].[VENTA] ADD  CONSTRAINT [DF_VENTA_DescPct]  DEFAULT ((0)) FOR [DescuentoPorcentaje]
GO
ALTER TABLE [dbo].[VENTA] ADD  CONSTRAINT [DF_VENTA_DescMonto]  DEFAULT ((0)) FOR [MontoDescuento]
GO
ALTER TABLE [dbo].[VENTA] ADD  CONSTRAINT [DF_VENTA_Anulada]  DEFAULT ((0)) FOR [Anulada]
GO
ALTER TABLE [dbo].[AJUSTE_INVENTARIO]  WITH CHECK ADD FOREIGN KEY([IdProducto])
REFERENCES [dbo].[PRODUCTO] ([IdProducto])
GO
ALTER TABLE [dbo].[AJUSTE_INVENTARIO]  WITH CHECK ADD FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[USUARIO] ([IdUsuario])
GO
ALTER TABLE [dbo].[COMPRA]  WITH CHECK ADD FOREIGN KEY([IdProveedor])
REFERENCES [dbo].[PROVEEDOR] ([IdProveedor])
GO
ALTER TABLE [dbo].[COMPRA]  WITH CHECK ADD FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[USUARIO] ([IdUsuario])
GO
ALTER TABLE [dbo].[COMPRA]  WITH CHECK ADD  CONSTRAINT [FK_COMPRA_UsuarioAnula] FOREIGN KEY([IdUsuarioAnula])
REFERENCES [dbo].[USUARIO] ([IdUsuario])
GO
ALTER TABLE [dbo].[COMPRA] CHECK CONSTRAINT [FK_COMPRA_UsuarioAnula]
GO
ALTER TABLE [dbo].[DETALLE_COMPRA]  WITH CHECK ADD FOREIGN KEY([IdCompra])
REFERENCES [dbo].[COMPRA] ([IdCompra])
GO
ALTER TABLE [dbo].[DETALLE_COMPRA]  WITH CHECK ADD FOREIGN KEY([IdProducto])
REFERENCES [dbo].[PRODUCTO] ([IdProducto])
GO
ALTER TABLE [dbo].[DETALLE_VENTA]  WITH CHECK ADD FOREIGN KEY([IdProducto])
REFERENCES [dbo].[PRODUCTO] ([IdProducto])
GO
ALTER TABLE [dbo].[DETALLE_VENTA]  WITH CHECK ADD FOREIGN KEY([IdVenta])
REFERENCES [dbo].[VENTA] ([IdVenta])
GO
ALTER TABLE [dbo].[PERMISO]  WITH CHECK ADD FOREIGN KEY([IdRol])
REFERENCES [dbo].[ROL] ([IdRol])
GO
ALTER TABLE [dbo].[PRODUCTO]  WITH CHECK ADD FOREIGN KEY([IdCategoria])
REFERENCES [dbo].[CATEGORIA] ([IdCategoria])
GO
ALTER TABLE [dbo].[USUARIO]  WITH CHECK ADD FOREIGN KEY([IdRol])
REFERENCES [dbo].[ROL] ([IdRol])
GO
ALTER TABLE [dbo].[VENTA]  WITH CHECK ADD FOREIGN KEY([IdCliente])
REFERENCES [dbo].[CLIENTE] ([IdCliente])
GO
ALTER TABLE [dbo].[VENTA]  WITH CHECK ADD FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[USUARIO] ([IdUsuario])
GO
ALTER TABLE [dbo].[VENTA]  WITH CHECK ADD  CONSTRAINT [FK_VENTA_UsuarioAnula] FOREIGN KEY([IdUsuarioAnula])
REFERENCES [dbo].[USUARIO] ([IdUsuario])
GO
ALTER TABLE [dbo].[VENTA] CHECK CONSTRAINT [FK_VENTA_UsuarioAnula]
GO
ALTER TABLE [dbo].[VENTA_PAGO]  WITH CHECK ADD FOREIGN KEY([IdVenta])
REFERENCES [dbo].[VENTA] ([IdVenta])
GO
ALTER TABLE [dbo].[AJUSTE_INVENTARIO]  WITH CHECK ADD CHECK  (([StockNuevo]>=(0)))
GO
ALTER TABLE [dbo].[CLIENTE]  WITH CHECK ADD  CONSTRAINT [CK_CLIENTE_Cumple] CHECK  (([CumpleDia] IS NULL AND [CumpleMes] IS NULL OR [CumpleDia] IS NOT NULL AND [CumpleMes] IS NOT NULL AND ([CumpleMes]>=(1) AND [CumpleMes]<=(12)) AND ([CumpleDia]>=(1) AND [CumpleDia]<=case when [CumpleMes]=(2) then (29) when [CumpleMes]=(11) OR [CumpleMes]=(9) OR [CumpleMes]=(6) OR [CumpleMes]=(4) then (30) else (31) end)))
GO
ALTER TABLE [dbo].[CLIENTE] CHECK CONSTRAINT [CK_CLIENTE_Cumple]
GO
ALTER TABLE [dbo].[COMPRA]  WITH CHECK ADD CHECK  (([MontoTotal]>=(0)))
GO
ALTER TABLE [dbo].[COMPRA]  WITH CHECK ADD  CONSTRAINT [CK_COMPRA_Metodo] CHECK  (([MetodoPago]='Transferencia' OR [MetodoPago]='Tarjeta' OR [MetodoPago]='Efectivo'))
GO
ALTER TABLE [dbo].[COMPRA] CHECK CONSTRAINT [CK_COMPRA_Metodo]
GO
ALTER TABLE [dbo].[DETALLE_COMPRA]  WITH CHECK ADD CHECK  (([Cantidad]>(0)))
GO
ALTER TABLE [dbo].[DETALLE_COMPRA]  WITH CHECK ADD CHECK  (([PrecioCompra]>=(0)))
GO
ALTER TABLE [dbo].[DETALLE_COMPRA]  WITH CHECK ADD CHECK  (([PrecioVenta]>=(0)))
GO
ALTER TABLE [dbo].[DETALLE_VENTA]  WITH CHECK ADD CHECK  (([Cantidad]>(0)))
GO
ALTER TABLE [dbo].[DETALLE_VENTA]  WITH CHECK ADD CHECK  (([PrecioVenta]>=(0)))
GO
ALTER TABLE [dbo].[PRODUCTO]  WITH CHECK ADD CHECK  (([PrecioCompra]>=(0)))
GO
ALTER TABLE [dbo].[PRODUCTO]  WITH CHECK ADD CHECK  (([PrecioVenta]>=(0)))
GO
ALTER TABLE [dbo].[PRODUCTO]  WITH CHECK ADD CHECK  (([Stock]>=(0)))
GO
ALTER TABLE [dbo].[PRODUCTO]  WITH CHECK ADD  CONSTRAINT [CK_PRODUCTO_Servicio] CHECK  (([EsServicio]=(0) OR [Stock]=(0) AND [StockMinimo]=(0)))
GO
ALTER TABLE [dbo].[PRODUCTO] CHECK CONSTRAINT [CK_PRODUCTO_Servicio]
GO
ALTER TABLE [dbo].[PRODUCTO]  WITH CHECK ADD  CONSTRAINT [CK_PRODUCTO_StockMinimo] CHECK  (([StockMinimo]>=(0)))
GO
ALTER TABLE [dbo].[PRODUCTO] CHECK CONSTRAINT [CK_PRODUCTO_StockMinimo]
GO
ALTER TABLE [dbo].[VENTA]  WITH CHECK ADD CHECK  (([MontoTotal]>=(0)))
GO
ALTER TABLE [dbo].[VENTA]  WITH CHECK ADD  CONSTRAINT [CK_VENTA_Descuento] CHECK  (([DescuentoPorcentaje]>=(0) AND [DescuentoPorcentaje]<=(100)))
GO
ALTER TABLE [dbo].[VENTA] CHECK CONSTRAINT [CK_VENTA_Descuento]
GO
ALTER TABLE [dbo].[VENTA]  WITH CHECK ADD  CONSTRAINT [CK_VENTA_Metodo] CHECK  (([MetodoPago]='Mixto' OR [MetodoPago]='Transferencia' OR [MetodoPago]='Tarjeta' OR [MetodoPago]='Efectivo'))
GO
ALTER TABLE [dbo].[VENTA] CHECK CONSTRAINT [CK_VENTA_Metodo]
GO
ALTER TABLE [dbo].[VENTA_PAGO]  WITH CHECK ADD CHECK  (([Monto]>=(0)))
GO
ALTER TABLE [dbo].[VENTA_PAGO]  WITH CHECK ADD  CONSTRAINT [CK_VENTA_PAGO_Metodo] CHECK  (([MetodoPago]='Transferencia' OR [MetodoPago]='Tarjeta' OR [MetodoPago]='Efectivo'))
GO
ALTER TABLE [dbo].[VENTA_PAGO] CHECK CONSTRAINT [CK_VENTA_PAGO_Metodo]
GO
/****** Objeto: StoredProcedure [dbo].[SP_AJUSTAR_INVENTARIO] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_AJUSTAR_INVENTARIO](
    @IdProducto int,
    @IdUsuario int,
    @StockEsperado int,        -- el stock que la persona vio en pantalla
    @StockNuevo int,           -- el conteo real
    @Motivo varchar(30),
    @Nota varchar(200),
    @Resultado bit OUTPUT,
    @Mensaje varchar(300) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @Resultado = 0;
    SET @Mensaje = '';
    SET @Motivo = LTRIM(RTRIM(ISNULL(@Motivo, '')));
    SET @Nota = NULLIF(LTRIM(RTRIM(ISNULL(@Nota, ''))), '');

    IF @Motivo = ''
    BEGIN SET @Mensaje = 'Selecciona el motivo del ajuste.'; RETURN; END

    IF @Motivo = 'Otro' AND LEN(ISNULL(@Nota, '')) < 5
    BEGIN SET @Mensaje = 'Con el motivo «Otro» escribe una nota que lo explique.'; RETURN; END

    IF @StockNuevo < 0 OR @StockNuevo > 1000000
    BEGIN SET @Mensaje = 'El conteo no es válido.'; RETURN; END

    IF @StockNuevo = @StockEsperado
    BEGIN SET @Mensaje = 'El conteo coincide con el sistema: no hay nada que ajustar.'; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM USUARIO WHERE IdUsuario = @IdUsuario AND Estado = 1)
    BEGIN SET @Mensaje = 'El usuario no es válido.'; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM PRODUCTO WHERE IdProducto = @IdProducto)
    BEGIN SET @Mensaje = 'El producto no existe.'; RETURN; END

    DECLARE @actual int;

    BEGIN TRY
        BEGIN TRAN;

        -- Solo cambia si el stock sigue siendo el que la persona vio al contar
        UPDATE PRODUCTO SET Stock = @StockNuevo
        WHERE IdProducto = @IdProducto AND Stock = @StockEsperado;

        IF @@ROWCOUNT = 0
        BEGIN
            SELECT @actual = Stock FROM PRODUCTO WHERE IdProducto = @IdProducto;
            ROLLBACK;
            SET @Mensaje = 'El stock cambió mientras tanto (ahora es ' + ISNULL(CAST(@actual AS varchar(12)), '?')
                         + '). Vuelve a contar.';
            RETURN;
        END

        INSERT INTO AJUSTE_INVENTARIO(IdProducto, IdUsuario, StockAnterior, StockNuevo, Motivo, Nota)
        VALUES(@IdProducto, @IdUsuario, @StockEsperado, @StockNuevo, @Motivo, @Nota);

        COMMIT;
        SET @Resultado = 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @Resultado = 0;
        SET @Mensaje = 'No se pudo registrar el ajuste: ' + ERROR_MESSAGE();
    END CATCH
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_ANULAR_COMPRA] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_ANULAR_COMPRA](
    @IdCompra int,
    @IdUsuario int,
    @Motivo varchar(200),
    @Resultado bit OUTPUT,
    @Mensaje varchar(300) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @Resultado = 0;
    SET @Mensaje = '';
    SET @Motivo = LTRIM(RTRIM(ISNULL(@Motivo, '')));

    IF LEN(@Motivo) < 5
    BEGIN SET @Mensaje = 'Escribe el motivo de la anulación (mínimo 5 letras).'; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM USUARIO WHERE IdUsuario = @IdUsuario AND Estado = 1)
    BEGIN SET @Mensaje = 'El usuario no es válido.'; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM COMPRA WHERE IdCompra = @IdCompra)
    BEGIN SET @Mensaje = 'La compra no existe.'; RETURN; END

    IF EXISTS (SELECT 1 FROM COMPRA WHERE IdCompra = @IdCompra AND Anulada = 1)
    BEGIN SET @Mensaje = 'La compra ya estaba anulada.'; RETURN; END

    DECLARE @lineas int, @filas int, @texto varchar(300);

    SELECT IdProducto, SUM(Cantidad) AS Cantidad
    INTO #lineas
    FROM DETALLE_COMPRA
    WHERE IdCompra = @IdCompra
    GROUP BY IdProducto;

    SELECT @lineas = COUNT(*) FROM #lineas;

    BEGIN TRY
        BEGIN TRAN;

        UPDATE COMPRA
        SET Anulada = 1, FechaAnulacion = GETDATE(), IdUsuarioAnula = @IdUsuario, MotivoAnulacion = @Motivo
        WHERE IdCompra = @IdCompra AND Anulada = 0;

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK;
            SET @Mensaje = 'La compra ya estaba anulada.';
            RETURN;
        END

        -- Solo descuenta si hay existencia suficiente: el stock nunca queda negativo
        UPDATE p SET p.Stock = p.Stock - l.Cantidad
        FROM PRODUCTO p
        INNER JOIN #lineas l ON l.IdProducto = p.IdProducto
        WHERE p.Stock >= l.Cantidad;
        SET @filas = @@ROWCOUNT;

        IF @filas <> @lineas
        BEGIN
            ROLLBACK;
            SELECT TOP 1 @texto = p.Nombre + ' (en existencia: ' + CAST(p.Stock AS varchar(12))
                                + ', comprados aquí: ' + CAST(l.Cantidad AS varchar(12)) + ')'
            FROM #lineas l
            INNER JOIN PRODUCTO p ON p.IdProducto = l.IdProducto
            WHERE p.Stock < l.Cantidad;
            SET @Mensaje = 'No se puede anular: ya se vendió parte de lo comprado de '
                         + ISNULL(@texto, 'un producto') + '.';
            RETURN;
        END

        COMMIT;
        SET @Resultado = 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @Resultado = 0;
        SET @Mensaje = 'No se pudo anular la compra: ' + ERROR_MESSAGE();
    END CATCH
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_ANULAR_VENTA] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_ANULAR_VENTA](
    @IdVenta int,
    @IdUsuario int,
    @Motivo varchar(200),
    @Resultado bit OUTPUT,
    @Mensaje varchar(300) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @Resultado = 0;
    SET @Mensaje = '';
    SET @Motivo = LTRIM(RTRIM(ISNULL(@Motivo, '')));

    IF LEN(@Motivo) < 5
    BEGIN SET @Mensaje = 'Escribe el motivo de la anulación (mínimo 5 letras).'; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM USUARIO WHERE IdUsuario = @IdUsuario AND Estado = 1)
    BEGIN SET @Mensaje = 'El usuario no es válido.'; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM VENTA WHERE IdVenta = @IdVenta)
    BEGIN SET @Mensaje = 'La venta no existe.'; RETURN; END

    IF EXISTS (SELECT 1 FROM VENTA WHERE IdVenta = @IdVenta AND Anulada = 1)
    BEGIN SET @Mensaje = 'La venta ya estaba anulada.'; RETURN; END

    SELECT IdProducto, SUM(Cantidad) AS Cantidad
    INTO #lineas
    FROM DETALLE_VENTA
    WHERE IdVenta = @IdVenta
    GROUP BY IdProducto;

    -- Los servicios no tienen stock que devolver
    IF EXISTS (SELECT 1 FROM #lineas l
               INNER JOIN PRODUCTO p ON p.IdProducto = l.IdProducto
               WHERE p.EsServicio = 0 AND CAST(p.Stock AS bigint) + l.Cantidad > 2147483647)
    BEGIN SET @Mensaje = 'El stock de un producto superaría el máximo permitido.'; RETURN; END

    BEGIN TRY
        BEGIN TRAN;

        -- La condición Anulada = 0 impide anular dos veces aunque dos personas lo intenten a la vez
        UPDATE VENTA
        SET Anulada = 1, FechaAnulacion = GETDATE(), IdUsuarioAnula = @IdUsuario, MotivoAnulacion = @Motivo
        WHERE IdVenta = @IdVenta AND Anulada = 0;

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK;
            SET @Mensaje = 'La venta ya estaba anulada.';
            RETURN;
        END

        UPDATE p SET p.Stock = p.Stock + l.Cantidad
        FROM PRODUCTO p
        INNER JOIN #lineas l ON l.IdProducto = p.IdProducto
        WHERE p.EsServicio = 0;

        COMMIT;
        SET @Resultado = 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @Resultado = 0;
        SET @Mensaje = 'No se pudo anular la venta: ' + ERROR_MESSAGE();
    END CATCH
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_CAMBIAR_CLAVE] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Cambiar la contraseña también limpia los intentos fallidos y el bloqueo
CREATE   PROCEDURE [dbo].[SP_CAMBIAR_CLAVE](
    @IdUsuario int,
    @ClaveHash varchar(200),
    @Resultado bit OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE USUARIO
    SET ClaveHash = @ClaveHash, IntentosFallidos = 0, BloqueadoHasta = NULL
    WHERE IdUsuario = @IdUsuario;
    SET @Resultado = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_CREAR_USUARIO] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Mismo nombre y parámetros que ya tenías; ahora valida el rol y cubre el choque de dos altas simultáneas
CREATE   PROCEDURE [dbo].[SP_CREAR_USUARIO](
    @NameUser varchar(50),
    @NombreCompleto varchar(100),
    @Correo varchar(100),
    @ClaveHash varchar(200),
    @IdRol int,
    @IdGenerado int OUTPUT,
    @Mensaje varchar(200) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET @IdGenerado = 0;
    SET @Mensaje = '';

    IF EXISTS (SELECT 1 FROM USUARIO WHERE NameUser = @NameUser)
    BEGIN SET @Mensaje = 'Ya existe un usuario con ese nombre.'; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM ROL WHERE IdRol = @IdRol)
    BEGIN SET @Mensaje = 'El rol no existe.'; RETURN; END

    BEGIN TRY
        INSERT INTO USUARIO(NameUser, NombreCompleto, Correo, ClaveHash, IdRol, Estado)
        VALUES(@NameUser, @NombreCompleto, @Correo, @ClaveHash, @IdRol, 1);
        SET @IdGenerado = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() IN (2627, 2601)
            SET @Mensaje = 'Ya existe un usuario con ese nombre.';
        ELSE
            SET @Mensaje = 'No se pudo crear el usuario: ' + ERROR_MESSAGE();
    END CATCH
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_DESBLOQUEAR_USUARIO] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_DESBLOQUEAR_USUARIO](@IdUsuario int)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE USUARIO SET IntentosFallidos = 0, BloqueadoHasta = NULL
    WHERE IdUsuario = @IdUsuario;
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_DETALLE_COMPRA] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_DETALLE_COMPRA](@IdCompra int)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.IdProducto, p.Codigo AS ProductoCodigo, p.Nombre AS ProductoNombre,
           d.PrecioCompra, d.PrecioVenta, d.Cantidad, d.MontoTotal
    FROM DETALLE_COMPRA d
    INNER JOIN PRODUCTO p ON p.IdProducto = d.IdProducto
    WHERE d.IdCompra = @IdCompra
    ORDER BY d.IdDetalleCompra
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_DETALLE_VENTA] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_DETALLE_VENTA](@IdVenta int, @IdUsuario int = NULL)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.IdProducto, p.Codigo AS ProductoCodigo, p.Nombre AS ProductoNombre,
           d.PrecioVenta, d.Cantidad, d.SubTotal
    FROM DETALLE_VENTA d
    INNER JOIN VENTA v ON v.IdVenta = d.IdVenta
    INNER JOIN PRODUCTO p ON p.IdProducto = d.IdProducto
    WHERE d.IdVenta = @IdVenta
      AND (@IdUsuario IS NULL OR v.IdUsuario = @IdUsuario)
    ORDER BY d.IdDetalleVenta
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_EDITAR_USUARIO] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_EDITAR_USUARIO](
    @IdUsuario int,
    @NameUser varchar(50),
    @NombreCompleto varchar(100),
    @Correo varchar(100),
    @IdRol int,
    @Estado bit,
    @Resultado bit OUTPUT,
    @Mensaje varchar(200) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @Resultado = 0;
    SET @Mensaje = '';

    IF NOT EXISTS (SELECT 1 FROM USUARIO WHERE IdUsuario = @IdUsuario)
    BEGIN SET @Mensaje = 'El usuario no existe.'; RETURN; END

    IF EXISTS (SELECT 1 FROM USUARIO WHERE NameUser = @NameUser AND IdUsuario <> @IdUsuario)
    BEGIN SET @Mensaje = 'Ya existe un usuario con ese nombre.'; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM ROL WHERE IdRol = @IdRol)
    BEGIN SET @Mensaje = 'El rol no existe.'; RETURN; END

    DECLARE @idAdmin int = (SELECT TOP 1 IdRol FROM ROL WHERE Descripcion = 'ADMINISTRADOR');
    DECLARE @otrosAdmins int;

    BEGIN TRY
        BEGIN TRAN;

        -- Cuenta los demás administradores activos y bloquea esas filas mientras se decide
        SELECT @otrosAdmins = COUNT(*)
        FROM USUARIO WITH (UPDLOCK, HOLDLOCK)
        WHERE IdRol = @idAdmin AND Estado = 1 AND IdUsuario <> @IdUsuario;

        -- Si este es el último administrador activo, no puede dejar de serlo
        IF @otrosAdmins = 0
           AND EXISTS (SELECT 1 FROM USUARIO WHERE IdUsuario = @IdUsuario AND IdRol = @idAdmin AND Estado = 1)
           AND NOT (@IdRol = @idAdmin AND @Estado = 1)
        BEGIN
            ROLLBACK;
            SET @Mensaje = 'Debe quedar al menos un administrador activo.';
            RETURN;
        END

        UPDATE USUARIO
        SET NameUser = @NameUser, NombreCompleto = @NombreCompleto, Correo = @Correo,
            IdRol = @IdRol, Estado = @Estado
        WHERE IdUsuario = @IdUsuario;

        COMMIT;
        SET @Resultado = 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        IF ERROR_NUMBER() IN (2627, 2601)
            SET @Mensaje = 'Ya existe un usuario con ese nombre.';
        ELSE
            SET @Mensaje = 'No se pudo guardar: ' + ERROR_MESSAGE();
    END CATCH
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_EXISTE_USUARIOS] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_EXISTE_USUARIOS]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM USUARIO
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_GUARDAR_CATEGORIA] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_GUARDAR_CATEGORIA](
    @IdCategoria int,              -- 0 = categoría nueva
    @Descripcion varchar(100),
    @Estado bit,
    @IdGenerado int OUTPUT,
    @Mensaje varchar(200) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET @IdGenerado = 0;
    SET @Mensaje = '';

    IF EXISTS (SELECT 1 FROM CATEGORIA
               WHERE Descripcion = @Descripcion AND IdCategoria <> @IdCategoria)
    BEGIN
        SET @Mensaje = 'Ya existe una categoría con ese nombre.';
        RETURN;
    END

    IF @IdCategoria = 0
    BEGIN
        INSERT INTO CATEGORIA(Descripcion, Estado) VALUES(@Descripcion, @Estado);
        SET @IdGenerado = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM CATEGORIA WHERE IdCategoria = @IdCategoria)
        BEGIN
            SET @Mensaje = 'La categoría no existe.';
            RETURN;
        END

        IF @Estado = 0 AND EXISTS (SELECT 1 FROM PRODUCTO
                                   WHERE IdCategoria = @IdCategoria AND Estado = 1)
        BEGIN
            SET @Mensaje = 'No se puede desactivar: tiene productos activos.';
            RETURN;
        END

        UPDATE CATEGORIA SET Descripcion = @Descripcion, Estado = @Estado
        WHERE IdCategoria = @IdCategoria;
        SET @IdGenerado = @IdCategoria;
    END
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_GUARDAR_CLIENTE] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_GUARDAR_CLIENTE](
    @IdCliente int,                -- 0 = cliente nuevo
    @NombreCompleto varchar(100),
    @Correo varchar(100),
    @Telefono varchar(50),
    @Estado bit,
    @IdGenerado int OUTPUT,
    @Mensaje varchar(200) OUTPUT,
    @CumpleDia tinyint = NULL,     -- opcionales: si no se envían, el cliente queda sin cumpleaños
    @CumpleMes tinyint = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @IdGenerado = 0;
    SET @Mensaje = '';

    IF (@CumpleDia IS NULL AND @CumpleMes IS NOT NULL) OR (@CumpleDia IS NOT NULL AND @CumpleMes IS NULL)
    BEGIN SET @Mensaje = 'Para guardar el cumpleaños elige el día y el mes.'; RETURN; END

    IF @CumpleMes IS NOT NULL
       AND (@CumpleMes NOT BETWEEN 1 AND 12
            OR @CumpleDia < 1
            OR @CumpleDia > CASE WHEN @CumpleMes = 2 THEN 29
                                 WHEN @CumpleMes IN (4, 6, 9, 11) THEN 30
                                 ELSE 31 END)
    BEGIN SET @Mensaje = 'La fecha de cumpleaños no es válida.'; RETURN; END

    IF @IdCliente = 0
    BEGIN
        DECLARE @n int, @codigo varchar(50);
        BEGIN TRAN;
        WHILE 1 = 1
        BEGIN
            UPDATE CONTADOR SET @n = Valor = Valor + 1 WHERE Nombre = 'CLIENTE';
            SET @codigo = 'CLI-' + FORMAT(@n, '0000');
            IF NOT EXISTS (SELECT 1 FROM CLIENTE WHERE Documento = @codigo) BREAK;
        END

        INSERT INTO CLIENTE(Documento, NombreCompleto, Correo, Telefono, Estado, CumpleDia, CumpleMes)
        VALUES(@codigo, @NombreCompleto, @Correo, @Telefono, @Estado, @CumpleDia, @CumpleMes);
        SET @IdGenerado = SCOPE_IDENTITY();
        COMMIT;
    END
    ELSE
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM CLIENTE WHERE IdCliente = @IdCliente)
        BEGIN
            SET @Mensaje = 'El cliente no existe.';
            RETURN;
        END

        -- El código NO se toca al editar
        UPDATE CLIENTE
        SET NombreCompleto = @NombreCompleto, Correo = @Correo, Telefono = @Telefono,
            Estado = @Estado, CumpleDia = @CumpleDia, CumpleMes = @CumpleMes
        WHERE IdCliente = @IdCliente;
        SET @IdGenerado = @IdCliente;
    END
END

GO
/****** Objeto: StoredProcedure [dbo].[SP_GUARDAR_PRODUCTO] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_GUARDAR_PRODUCTO](
    @IdProducto int,               -- 0 = nuevo
    @Nombre varchar(100),
    @IdCategoria int,
    @Stock int,                    -- solo se usa al crear (un servicio siempre queda en 0)
    @StockMinimo int,
    @PrecioCompra decimal(10,2),
    @PrecioVenta decimal(10,2),
    @Descripcion varchar(200),
    @Estado bit,
    @IdGenerado int OUTPUT,
    @Mensaje varchar(200) OUTPUT,
    @EsServicio bit = 0
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @IdGenerado = 0;
    SET @Mensaje = '';
    SET @EsServicio = ISNULL(@EsServicio, 0);

    -- Un servicio no maneja inventario
    IF @EsServicio = 1
    BEGIN
        SET @Stock = 0;
        SET @StockMinimo = 0;
    END

    -- La categoría debe existir y estar activa
    -- (o ser la que ya tenía, aunque se haya desactivado después)
    IF NOT EXISTS (
        SELECT 1 FROM CATEGORIA c
        WHERE c.IdCategoria = @IdCategoria
          AND (c.Estado = 1
               OR EXISTS (SELECT 1 FROM PRODUCTO p
                          WHERE p.IdProducto = @IdProducto AND p.IdCategoria = @IdCategoria)))
    BEGIN
        SET @Mensaje = 'La categoría no existe o está inactiva.';
        RETURN;
    END

    IF @IdProducto = 0
    BEGIN
        DECLARE @n int, @codigo varchar(100), @prefijo varchar(4), @contador varchar(20);
        SET @prefijo = CASE WHEN @EsServicio = 1 THEN 'SRV-' ELSE 'PRD-' END;
        SET @contador = CASE WHEN @EsServicio = 1 THEN 'SERVICIO' ELSE 'PRODUCTO' END;

        BEGIN TRAN;
        WHILE 1 = 1
        BEGIN
            UPDATE CONTADOR SET @n = Valor = Valor + 1 WHERE Nombre = @contador;
            SET @codigo = @prefijo + FORMAT(@n, '0000');
            IF NOT EXISTS (SELECT 1 FROM PRODUCTO WHERE Codigo = @codigo) BREAK;
        END

        INSERT INTO PRODUCTO(Codigo, Nombre, IdCategoria, Stock, StockMinimo,
                             PrecioCompra, PrecioVenta, Descripcion, Estado, EsServicio)
        VALUES(@codigo, @Nombre, @IdCategoria, @Stock, @StockMinimo,
               @PrecioCompra, @PrecioVenta, @Descripcion, @Estado, @EsServicio);
        SET @IdGenerado = SCOPE_IDENTITY();
        COMMIT;
    END
    ELSE
    BEGIN
        DECLARE @tipoActual bit;
        SELECT @tipoActual = EsServicio FROM PRODUCTO WHERE IdProducto = @IdProducto;

        IF @tipoActual IS NULL
        BEGIN
            SET @Mensaje = 'El producto no existe.';
            RETURN;
        END

        IF @tipoActual <> @EsServicio
        BEGIN
            SET @Mensaje = 'No se puede cambiar entre producto y servicio. Crea uno nuevo y desactiva este.';
            RETURN;
        END

        -- Ni el código ni el stock se tocan aquí
        UPDATE PRODUCTO
        SET Nombre = @Nombre, IdCategoria = @IdCategoria, StockMinimo = @StockMinimo,
            PrecioCompra = @PrecioCompra, PrecioVenta = @PrecioVenta,
            Descripcion = @Descripcion, Estado = @Estado
        WHERE IdProducto = @IdProducto;
        SET @IdGenerado = @IdProducto;
    END
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_GUARDAR_PROVEEDOR] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ===== PROVEEDOR =====
CREATE   PROCEDURE [dbo].[SP_GUARDAR_PROVEEDOR](
    @IdProveedor int,              -- 0 = proveedor nuevo
    @RazonSocial varchar(100),
    @Correo varchar(100),
    @Telefono varchar(50),
    @Estado bit,
    @IdGenerado int OUTPUT,
    @Mensaje varchar(200) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @IdGenerado = 0;
    SET @Mensaje = '';

    IF @IdProveedor = 0
    BEGIN
        DECLARE @n int, @codigo varchar(50);
        BEGIN TRAN;
        WHILE 1 = 1
        BEGIN
            UPDATE CONTADOR SET @n = Valor = Valor + 1 WHERE Nombre = 'PROVEEDOR';
            SET @codigo = 'PRV-' + FORMAT(@n, '0000');
            IF NOT EXISTS (SELECT 1 FROM PROVEEDOR WHERE Documento = @codigo) BREAK;
        END

        INSERT INTO PROVEEDOR(Documento, RazonSocial, Correo, Telefono, Estado)
        VALUES(@codigo, @RazonSocial, @Correo, @Telefono, @Estado);
        SET @IdGenerado = SCOPE_IDENTITY();
        COMMIT;
    END
    ELSE
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM PROVEEDOR WHERE IdProveedor = @IdProveedor)
        BEGIN
            SET @Mensaje = 'El proveedor no existe.';
            RETURN;
        END

        UPDATE PROVEEDOR
        SET RazonSocial = @RazonSocial, Correo = @Correo,
            Telefono = @Telefono, Estado = @Estado
        WHERE IdProveedor = @IdProveedor;
        SET @IdGenerado = @IdProveedor;
    END
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_LISTAR_AJUSTES] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_LISTAR_AJUSTES](@Desde date, @Hasta date)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.IdAjuste, a.FechaRegistro, p.Codigo, p.Nombre,
           a.StockAnterior, a.StockNuevo, a.Diferencia, a.Motivo, a.Nota,
           u.NombreCompleto AS UsuarioNombre
    FROM AJUSTE_INVENTARIO a
    INNER JOIN PRODUCTO p ON p.IdProducto = a.IdProducto
    INNER JOIN USUARIO u ON u.IdUsuario = a.IdUsuario
    WHERE a.FechaRegistro >= @Desde AND a.FechaRegistro < DATEADD(DAY, 1, @Hasta)
    ORDER BY a.FechaRegistro DESC, a.IdAjuste DESC
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_LISTAR_CATEGORIAS] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_LISTAR_CATEGORIAS]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdCategoria, Descripcion, Estado
    FROM CATEGORIA
    ORDER BY Descripcion
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_LISTAR_CLIENTES] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_LISTAR_CLIENTES]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdCliente, Documento, NombreCompleto, Correo, Telefono, Estado,
           CumpleDia, CumpleMes
    FROM CLIENTE
    ORDER BY NombreCompleto
END

GO
/****** Objeto: StoredProcedure [dbo].[SP_LISTAR_COMPRAS] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_LISTAR_COMPRAS](@Desde date, @Hasta date)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.IdCompra, c.NumeroDocumento, c.TipoDocumento, c.MetodoPago, c.ReferenciaPago,
           c.MontoTotal, c.FechaRegistro,
           c.IdProveedor, p.RazonSocial AS ProveedorNombre,
           c.IdUsuario, u.NombreCompleto AS UsuarioNombre,
           c.Anulada, c.FechaAnulacion, c.MotivoAnulacion, ua.NombreCompleto AS AnuladaPor
    FROM COMPRA c
    INNER JOIN PROVEEDOR p ON p.IdProveedor = c.IdProveedor
    INNER JOIN USUARIO u ON u.IdUsuario = c.IdUsuario
    LEFT JOIN USUARIO ua ON ua.IdUsuario = c.IdUsuarioAnula
    WHERE c.FechaRegistro >= @Desde AND c.FechaRegistro < DATEADD(DAY, 1, @Hasta)
    ORDER BY c.FechaRegistro DESC, c.IdCompra DESC
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_LISTAR_PAGOS] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7) Todos los pagos de las ventas vigentes (no anuladas) de un periodo, para los reportes
CREATE   PROCEDURE [dbo].[SP_LISTAR_PAGOS](@Desde date, @Hasta date)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.IdVenta, p.MetodoPago, p.Monto
    FROM VENTA_PAGO p
    INNER JOIN VENTA v ON v.IdVenta = p.IdVenta
    WHERE v.Anulada = 0
      AND v.FechaRegistro >= @Desde AND v.FechaRegistro < DATEADD(DAY, 1, @Hasta)
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_LISTAR_PERMISOS] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_LISTAR_PERMISOS](@IdRol int)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT NombreMenu FROM PERMISO WHERE IdRol = @IdRol
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_LISTAR_PRODUCTOS] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_LISTAR_PRODUCTOS]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.IdProducto, p.Codigo, p.Nombre, p.IdCategoria,
           c.Descripcion AS CategoriaDescripcion,
           p.Stock, p.StockMinimo, p.PrecioCompra, p.PrecioVenta,
           p.Descripcion, p.Estado, p.EsServicio
    FROM PRODUCTO p
    INNER JOIN CATEGORIA c ON c.IdCategoria = p.IdCategoria
    ORDER BY p.Nombre
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_LISTAR_PROVEEDORES] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_LISTAR_PROVEEDORES]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdProveedor, Documento, RazonSocial, Correo, Telefono, Estado
    FROM PROVEEDOR
    ORDER BY RazonSocial
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_LISTAR_ROLES] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_LISTAR_ROLES]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdRol, Descripcion FROM ROL ORDER BY IdRol
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_LISTAR_USUARIOS] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_LISTAR_USUARIOS]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.IdUsuario, u.NameUser, u.NombreCompleto, u.Correo, u.Estado, u.FechaRegistro,
           u.IdRol, r.Descripcion AS RolDescripcion,
           CASE WHEN u.BloqueadoHasta IS NOT NULL AND u.BloqueadoHasta > GETDATE()
                THEN 1 ELSE 0 END AS Bloqueado
    FROM USUARIO u
    INNER JOIN ROL r ON r.IdRol = u.IdRol
    ORDER BY u.NombreCompleto
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_LISTAR_VENTAS] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_LISTAR_VENTAS](@Desde date, @Hasta date, @IdUsuario int = NULL)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT v.IdVenta, v.NumeroDocumento, v.TipoDocumento, v.MetodoPago, v.ReferenciaPago,
           v.Subtotal, v.DescuentoPorcentaje, v.MontoDescuento, v.MontoTotal,
           v.MontoPago, v.MontoCambio, v.FechaRegistro,
           v.IdCliente, ISNULL(c.NombreCompleto, 'Público en general') AS ClienteNombre,
           v.IdUsuario, u.NombreCompleto AS UsuarioNombre,
           v.Anulada, v.FechaAnulacion, v.MotivoAnulacion, ua.NombreCompleto AS AnuladaPor
    FROM VENTA v
    INNER JOIN USUARIO u ON u.IdUsuario = v.IdUsuario
    LEFT JOIN CLIENTE c ON c.IdCliente = v.IdCliente
    LEFT JOIN USUARIO ua ON ua.IdUsuario = v.IdUsuarioAnula
    WHERE v.FechaRegistro >= @Desde AND v.FechaRegistro < DATEADD(DAY, 1, @Hasta)
      AND (@IdUsuario IS NULL OR v.IdUsuario = @IdUsuario)
    ORDER BY v.FechaRegistro DESC, v.IdVenta DESC
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_LOGIN_EXITOSO] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_LOGIN_EXITOSO](@IdUsuario int)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE USUARIO SET IntentosFallidos = 0, BloqueadoHasta = NULL
    WHERE IdUsuario = @IdUsuario
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_OBTENER_LOGIN] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Datos del usuario para validar el login (la clave se verifica en C#)
CREATE PROCEDURE [dbo].[SP_OBTENER_LOGIN](@NameUser varchar(50))
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.IdUsuario, u.NameUser, u.NombreCompleto, u.Correo, u.ClaveHash,
           u.Estado, u.FechaRegistro, u.IdRol, r.Descripcion AS RolDescripcion,
           CASE WHEN u.BloqueadoHasta IS NOT NULL AND u.BloqueadoHasta > GETDATE()
                THEN 1 ELSE 0 END AS Bloqueado
    FROM USUARIO u
    INNER JOIN ROL r ON r.IdRol = u.IdRol
    WHERE u.NameUser = @NameUser
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_PAGOS_VENTA] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 6) Las formas de pago de una venta (para el detalle, el ticket y el comprobante)
CREATE   PROCEDURE [dbo].[SP_PAGOS_VENTA](@IdVenta int, @IdUsuario int = NULL)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.MetodoPago, p.Monto, p.ReferenciaPago
    FROM VENTA_PAGO p
    INNER JOIN VENTA v ON v.IdVenta = p.IdVenta
    WHERE p.IdVenta = @IdVenta
      AND (@IdUsuario IS NULL OR v.IdUsuario = @IdUsuario)
    ORDER BY p.IdVentaPago
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_REGISTRAR_COMPRA] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_REGISTRAR_COMPRA](
    @IdUsuario int,
    @IdProveedor int,
    @TipoDocumento varchar(50),
    @MetodoPago varchar(30) = 'Efectivo',
    @ReferenciaPago varchar(50) = NULL,
    @Detalle EDetalle_Compra READONLY,
    @IdGenerado int OUTPUT,
    @NumeroDocumento varchar(50) OUTPUT,
    @Mensaje varchar(300) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @IdGenerado = 0;
    SET @NumeroDocumento = '';
    SET @Mensaje = '';
    SET @ReferenciaPago = NULLIF(LTRIM(RTRIM(ISNULL(@ReferenciaPago, ''))), '');

    IF NOT EXISTS (SELECT 1 FROM USUARIO WHERE IdUsuario = @IdUsuario AND Estado = 1)
    BEGIN SET @Mensaje = 'El usuario no es válido.'; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM PROVEEDOR WHERE IdProveedor = @IdProveedor AND Estado = 1)
    BEGIN SET @Mensaje = 'El proveedor no existe o está inactivo.'; RETURN; END

    IF LEN(LTRIM(RTRIM(ISNULL(@TipoDocumento, '')))) = 0
    BEGIN SET @Mensaje = 'El tipo de documento es obligatorio.'; RETURN; END

    IF ISNULL(@MetodoPago, '') NOT IN ('Efectivo', 'Tarjeta', 'Transferencia')
    BEGIN SET @Mensaje = 'El método de pago no es válido.'; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM @Detalle)
    BEGIN SET @Mensaje = 'La compra no tiene productos.'; RETURN; END

    IF EXISTS (SELECT IdProducto FROM @Detalle GROUP BY IdProducto HAVING COUNT(*) > 1)
    BEGIN SET @Mensaje = 'Hay productos repetidos en la compra.'; RETURN; END

    IF EXISTS (SELECT 1 FROM @Detalle WHERE Cantidad <= 0 OR PrecioCompra < 0 OR PrecioVenta <= 0)
    BEGIN SET @Mensaje = 'Hay cantidades o precios no válidos.'; RETURN; END

    IF EXISTS (SELECT 1 FROM @Detalle d
               LEFT JOIN PRODUCTO p ON p.IdProducto = d.IdProducto
               WHERE p.IdProducto IS NULL OR p.Estado = 0)
    BEGIN SET @Mensaje = 'Hay productos que no existen o están inactivos.'; RETURN; END

    IF EXISTS (SELECT 1 FROM @Detalle d
               INNER JOIN PRODUCTO p ON p.IdProducto = d.IdProducto
               WHERE CAST(p.Stock AS bigint) + d.Cantidad > 2147483647)
    BEGIN SET @Mensaje = 'El stock de un producto superaría el máximo permitido.'; RETURN; END

    DECLARE @total decimal(18,2) = (SELECT SUM(Cantidad * PrecioCompra) FROM @Detalle);
    IF @total > 99999999.99
    BEGIN SET @Mensaje = 'El total de la compra es demasiado grande.'; RETURN; END

    DECLARE @n int, @numero varchar(50), @idCompra int;

    BEGIN TRY
        BEGIN TRAN;

        WHILE 1 = 1
        BEGIN
            UPDATE CONTADOR SET @n = Valor = Valor + 1 WHERE Nombre = 'COMPRA';
            SET @numero = 'COM-' + FORMAT(@n, '0000');
            IF NOT EXISTS (SELECT 1 FROM COMPRA WHERE NumeroDocumento = @numero) BREAK;
        END

        INSERT INTO COMPRA(IdUsuario, IdProveedor, TipoDocumento, NumeroDocumento, MontoTotal,
                           MetodoPago, ReferenciaPago)
        VALUES(@IdUsuario, @IdProveedor, LTRIM(RTRIM(@TipoDocumento)), @numero, @total,
               @MetodoPago, @ReferenciaPago);
        SET @idCompra = SCOPE_IDENTITY();

        INSERT INTO DETALLE_COMPRA(IdCompra, IdProducto, PrecioCompra, PrecioVenta, Cantidad, MontoTotal)
        SELECT @idCompra, IdProducto, PrecioCompra, PrecioVenta, Cantidad, Cantidad * PrecioCompra
        FROM @Detalle;

        UPDATE p
        SET p.Stock = p.Stock + d.Cantidad,
            p.PrecioCompra = d.PrecioCompra,
            p.PrecioVenta = d.PrecioVenta
        FROM PRODUCTO p
        INNER JOIN @Detalle d ON d.IdProducto = p.IdProducto;

        COMMIT;
        SET @IdGenerado = @idCompra;
        SET @NumeroDocumento = @numero;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @IdGenerado = 0;
        SET @NumeroDocumento = '';
        SET @Mensaje = 'No se pudo registrar la compra: ' + ERROR_MESSAGE();
    END CATCH
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_REGISTRAR_INTENTO_FALLIDO] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 5 fallos seguidos = bloqueo de 5 minutos
CREATE PROCEDURE [dbo].[SP_REGISTRAR_INTENTO_FALLIDO](@IdUsuario int)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE USUARIO
    SET BloqueadoHasta = CASE WHEN IntentosFallidos + 1 >= 5
                              THEN DATEADD(MINUTE, 5, GETDATE()) ELSE BloqueadoHasta END,
        IntentosFallidos = CASE WHEN IntentosFallidos + 1 >= 5
                                THEN 0 ELSE IntentosFallidos + 1 END
    WHERE IdUsuario = @IdUsuario
END
GO
/****** Objeto: StoredProcedure [dbo].[SP_REGISTRAR_VENTA] Fecha de script: 10/10/2026 8:27:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 5) Registrar una venta con una o dos formas de pago
CREATE   PROCEDURE [dbo].[SP_REGISTRAR_VENTA](
    @IdUsuario int,
    @IdCliente int,                        -- NULL = público en general
    @DescuentoPorcentaje decimal(5,2),
    @MontoPago decimal(10,2),              -- efectivo que entrega el cliente (solo si alguna parte se paga en efectivo)
    @Detalle EDetalle_Venta READONLY,
    @Pagos EPago_Venta READONLY,           -- una o dos formas de pago; deben sumar el total
    @IdGenerado int OUTPUT,
    @NumeroDocumento varchar(50) OUTPUT,
    @MontoTotal decimal(10,2) OUTPUT,
    @MontoCambio decimal(10,2) OUTPUT,
    @Mensaje varchar(300) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @IdGenerado = 0;
    SET @NumeroDocumento = '';
    SET @MontoTotal = 0;
    SET @MontoCambio = 0;
    SET @Mensaje = '';
    SET @DescuentoPorcentaje = ISNULL(@DescuentoPorcentaje, 0);

    -- ===== Validaciones previas =====
    IF NOT EXISTS (SELECT 1 FROM USUARIO WHERE IdUsuario = @IdUsuario AND Estado = 1)
    BEGIN SET @Mensaje = 'El usuario no es válido.'; RETURN; END

    IF @IdCliente IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM CLIENTE WHERE IdCliente = @IdCliente AND Estado = 1)
    BEGIN SET @Mensaje = 'El cliente no existe o está inactivo.'; RETURN; END

    IF @DescuentoPorcentaje < 0 OR @DescuentoPorcentaje > 100
    BEGIN SET @Mensaje = 'El descuento debe estar entre 0 y 100%.'; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM @Detalle)
    BEGIN SET @Mensaje = 'La venta no tiene productos.'; RETURN; END

    IF EXISTS (SELECT IdProducto FROM @Detalle GROUP BY IdProducto HAVING COUNT(*) > 1)
    BEGIN SET @Mensaje = 'Hay productos repetidos en la venta.'; RETURN; END

    IF EXISTS (SELECT 1 FROM @Detalle WHERE Cantidad <= 0 OR PrecioVenta <= 0)
    BEGIN SET @Mensaje = 'Hay cantidades o precios no válidos.'; RETURN; END

    IF EXISTS (SELECT 1 FROM @Detalle d
               LEFT JOIN PRODUCTO p ON p.IdProducto = d.IdProducto
               WHERE p.IdProducto IS NULL OR p.Estado = 0)
    BEGIN SET @Mensaje = 'Hay productos que no existen o están inactivos.'; RETURN; END

    -- Formas de pago: una o dos, métodos válidos y distintos, y en un pago mixto cada parte debe ser mayor a 0
    DECLARE @nPagos int = (SELECT COUNT(*) FROM @Pagos);

    IF @nPagos NOT BETWEEN 1 AND 2
    BEGIN SET @Mensaje = 'La venta debe tener una o dos formas de pago.'; RETURN; END

    IF EXISTS (SELECT 1 FROM @Pagos WHERE MetodoPago NOT IN ('Efectivo', 'Tarjeta', 'Transferencia'))
    BEGIN SET @Mensaje = 'El método de pago no es válido.'; RETURN; END

    IF @nPagos = 2 AND (SELECT COUNT(DISTINCT MetodoPago) FROM @Pagos) < 2
    BEGIN SET @Mensaje = 'En un pago mixto los dos métodos deben ser distintos.'; RETURN; END

    IF EXISTS (SELECT 1 FROM @Pagos WHERE Monto < 0 OR (@nPagos = 2 AND Monto = 0))
    BEGIN SET @Mensaje = 'Los montos de pago no son válidos.'; RETURN; END

    DECLARE @n int, @numero varchar(50), @idVenta int, @filas int, @lineas int, @texto varchar(300);
    DECLARE @subtotal decimal(18,2), @descuento decimal(18,2), @total decimal(18,2);
    DECLARE @sumaPagos decimal(18,2), @efectivo decimal(18,2);
    DECLARE @pago decimal(10,2), @cambio decimal(10,2);
    DECLARE @metodo varchar(30), @referencia varchar(50);

    -- Solo los productos con inventario descuentan stock; los servicios no
    SELECT @lineas = COUNT(*)
    FROM @Detalle d
    INNER JOIN PRODUCTO p ON p.IdProducto = d.IdProducto
    WHERE p.EsServicio = 0;

    BEGIN TRY
        BEGIN TRAN;

        -- 1) Descuenta el stock. La condición impide que quede negativo
        --    y deja bloqueadas las filas hasta terminar la venta
        UPDATE p SET p.Stock = p.Stock - d.Cantidad
        FROM PRODUCTO p
        INNER JOIN @Detalle d ON d.IdProducto = p.IdProducto
        WHERE p.EsServicio = 0 AND p.Stock >= d.Cantidad;
        SET @filas = @@ROWCOUNT;

        IF @filas <> @lineas
        BEGIN
            ROLLBACK;
            SELECT TOP 1 @texto = p.Nombre + ' (disponible: ' + CAST(p.Stock AS varchar(12)) + ')'
            FROM @Detalle d
            INNER JOIN PRODUCTO p ON p.IdProducto = d.IdProducto
            WHERE p.EsServicio = 0 AND p.Stock < d.Cantidad;
            SET @Mensaje = 'Stock insuficiente: ' + ISNULL(@texto, 'revisa las existencias') + '.';
            RETURN;
        END

        -- 2) El precio enviado debe ser el vigente
        SET @texto = NULL;
        SELECT TOP 1 @texto = p.Nombre
        FROM @Detalle d
        INNER JOIN PRODUCTO p ON p.IdProducto = d.IdProducto
        WHERE d.PrecioVenta <> p.PrecioVenta;

        IF @texto IS NOT NULL
        BEGIN
            ROLLBACK;
            SET @Mensaje = 'El precio de ' + @texto + ' cambió. Revisa el total e intenta de nuevo.';
            RETURN;
        END

        -- 3) Totales
        SELECT @subtotal = SUM(Cantidad * PrecioVenta) FROM @Detalle;

        IF @subtotal > 99999999.99
        BEGIN
            ROLLBACK;
            SET @Mensaje = 'El total de la venta es demasiado grande.';
            RETURN;
        END

        SET @descuento = ROUND(@subtotal * @DescuentoPorcentaje / 100, 2);
        SET @total = @subtotal - @descuento;

        -- 4) Los pagos deben sumar exactamente el total
        SELECT @sumaPagos = SUM(Monto) FROM @Pagos;

        IF @sumaPagos <> @total
        BEGIN
            ROLLBACK;
            SET @Mensaje = 'Los pagos suman ' + CONVERT(varchar(20), @sumaPagos)
                         + ' y el total es ' + CONVERT(varchar(20), @total) + '.';
            RETURN;
        END

        -- 5) Efectivo: si una parte se paga en efectivo, lo que entrega el cliente debe alcanzar para esa parte
        SELECT @efectivo = SUM(Monto) FROM @Pagos WHERE MetodoPago = 'Efectivo';   -- NULL si no hay efectivo

        IF @efectivo IS NOT NULL
        BEGIN
            IF @MontoPago IS NULL OR @MontoPago < @efectivo
            BEGIN
                ROLLBACK;
                SET @Mensaje = 'El efectivo recibido es menor a lo que se cobra en efectivo.';
                RETURN;
            END
            SET @cambio = @MontoPago - @efectivo;
        END
        ELSE
        BEGIN
            SET @cambio = 0;
        END

        -- MontoPago guarda todo lo que entregó el cliente; siempre se cumple MontoPago - MontoCambio = MontoTotal
        SET @pago = @total + @cambio;

        -- 6) Cómo queda anotado en la venta: el método (o «Mixto») y las referencias juntas
        SELECT @metodo = CASE WHEN @nPagos = 1 THEN MIN(MetodoPago) ELSE 'Mixto' END FROM @Pagos;
        SELECT @referencia = LEFT(STRING_AGG(NULLIF(LTRIM(RTRIM(ReferenciaPago)), ''), ' / '), 50) FROM @Pagos;

        -- 7) Número automático
        WHILE 1 = 1
        BEGIN
            UPDATE CONTADOR SET @n = Valor = Valor + 1 WHERE Nombre = 'VENTA';
            SET @numero = 'VEN-' + FORMAT(@n, '0000');
            IF NOT EXISTS (SELECT 1 FROM VENTA WHERE TipoDocumento = 'Ticket' AND NumeroDocumento = @numero) BREAK;
        END

        -- 8) Guardar la venta, su detalle y sus pagos
        INSERT INTO VENTA(IdUsuario, IdCliente, TipoDocumento, NumeroDocumento, MontoPago, MontoCambio,
                          MontoTotal, MetodoPago, ReferenciaPago, Subtotal, DescuentoPorcentaje, MontoDescuento)
        VALUES(@IdUsuario, @IdCliente, 'Ticket', @numero, @pago, @cambio,
               @total, @metodo, @referencia, @subtotal, @DescuentoPorcentaje, @descuento);
        SET @idVenta = SCOPE_IDENTITY();

        INSERT INTO DETALLE_VENTA(IdVenta, IdProducto, PrecioVenta, Cantidad, SubTotal)
        SELECT @idVenta, IdProducto, PrecioVenta, Cantidad, Cantidad * PrecioVenta
        FROM @Detalle;

        INSERT INTO VENTA_PAGO(IdVenta, MetodoPago, Monto, ReferenciaPago)
        SELECT @idVenta, MetodoPago, Monto, NULLIF(LTRIM(RTRIM(ReferenciaPago)), '')
        FROM @Pagos;

        COMMIT;

        SET @IdGenerado = @idVenta;
        SET @NumeroDocumento = @numero;
        SET @MontoTotal = @total;
        SET @MontoCambio = @cambio;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @IdGenerado = 0;
        SET @NumeroDocumento = '';
        SET @MontoTotal = 0;
        SET @MontoCambio = 0;
        SET @Mensaje = 'No se pudo registrar la venta: ' + ERROR_MESSAGE();
    END CATCH
END
GO
USE [master]
GO
ALTER DATABASE [DBBELISA] SET  READ_WRITE 
GO
