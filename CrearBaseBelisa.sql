/*
  BELISA · Punto de venta
  Script de creación de la base de datos · Versión 1.0

  Crea la base DBBELISA desde cero con todas sus tablas VACÍAS.
  Solo carga los datos de configuración que el programa necesita para arrancar:
    - Roles (ADMINISTRADOR y EMPLEADO)
    - Permisos de cada rol
    - Contadores de la numeración automática (PRD-0001, VEN-0001...)
  NO crea ningún usuario: el programa pide crear al administrador la primera vez que se abre.

  Es seguro ejecutarlo más de una vez: si la base ya existe, no hace nada.

  Para crearla con otro nombre (por ejemplo, para hacer pruebas), usa Ctrl+H y reemplaza
  todo el texto DBBELISA por el nombre nuevo (por ejemplo DBBELISA_PRUEBA).
*/

USE master
GO

IF DB_ID(N'DBBELISA') IS NOT NULL
BEGIN
    PRINT N'La base de datos DBBELISA ya existe. No se hizo ningún cambio.';
    SET NOEXEC ON;   -- el resto del script se omite
END
GO

CREATE DATABASE DBBELISA
GO

USE DBBELISA
GO

/* ============================== TABLAS ============================== */

CREATE TABLE ROL(
    IdRol int IDENTITY PRIMARY KEY,
    Descripcion varchar(50) NOT NULL UNIQUE,
    FechaRegistro datetime NOT NULL DEFAULT GETDATE()
)
GO

CREATE TABLE PERMISO(
    IdPermiso int IDENTITY PRIMARY KEY,
    IdRol int NOT NULL REFERENCES ROL(IdRol),
    NombreMenu varchar(100) NOT NULL,
    FechaRegistro datetime NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_PERMISO UNIQUE (IdRol, NombreMenu)
)
GO

CREATE TABLE USUARIO(
    IdUsuario int IDENTITY PRIMARY KEY,
    NameUser varchar(50) NOT NULL UNIQUE,
    NombreCompleto varchar(100) NOT NULL,
    Correo varchar(100) NULL,
    ClaveHash varchar(200) NOT NULL,          -- PBKDF2 con sal; la genera el programa
    IdRol int NOT NULL REFERENCES ROL(IdRol),
    Estado bit NOT NULL DEFAULT 1,
    IntentosFallidos int NOT NULL DEFAULT 0,
    BloqueadoHasta datetime NULL,
    FechaRegistro datetime NOT NULL DEFAULT GETDATE()
)
GO

CREATE TABLE CATEGORIA(
    IdCategoria int IDENTITY PRIMARY KEY,
    Descripcion varchar(100) NOT NULL UNIQUE,
    Estado bit NOT NULL DEFAULT 1,
    FechaRegistro datetime NOT NULL DEFAULT GETDATE()
)
GO

CREATE TABLE PRODUCTO(
    IdProducto int IDENTITY PRIMARY KEY,
    Codigo varchar(100) NOT NULL UNIQUE,      -- PRD-0001 (producto) o SRV-0001 (servicio)
    Nombre varchar(100) NOT NULL,
    IdCategoria int NOT NULL REFERENCES CATEGORIA(IdCategoria),
    Stock int NOT NULL DEFAULT 0 CHECK (Stock >= 0),
    StockMinimo int NOT NULL DEFAULT 5 CHECK (StockMinimo >= 0),
    PrecioCompra decimal(10,2) NOT NULL DEFAULT 0 CHECK (PrecioCompra >= 0),   -- en un servicio es el costo estimado
    PrecioVenta decimal(10,2) NOT NULL DEFAULT 0 CHECK (PrecioVenta >= 0),
    Descripcion varchar(200) NULL,
    Estado bit NOT NULL DEFAULT 1,
    EsServicio bit NOT NULL DEFAULT 0,
    FechaRegistro datetime NOT NULL DEFAULT GETDATE(),
    -- Un servicio nunca puede tener existencias: lo garantiza la base misma
    CONSTRAINT CK_PRODUCTO_Servicio CHECK (EsServicio = 0 OR (Stock = 0 AND StockMinimo = 0))
)
GO

CREATE TABLE CLIENTE(
    IdCliente int IDENTITY PRIMARY KEY,
    Documento varchar(50) NULL,               -- código automático CLI-0001
    NombreCompleto varchar(100) NOT NULL,
    Correo varchar(100) NULL,
    Telefono varchar(50) NULL,
    Estado bit NOT NULL DEFAULT 1,
    FechaRegistro datetime NOT NULL DEFAULT GETDATE()
)
GO
CREATE UNIQUE INDEX UX_CLIENTE_Documento ON CLIENTE(Documento) WHERE Documento IS NOT NULL
GO

CREATE TABLE PROVEEDOR(
    IdProveedor int IDENTITY PRIMARY KEY,
    Documento varchar(50) NULL,               -- código automático PRV-0001
    RazonSocial varchar(100) NOT NULL,
    Correo varchar(100) NULL,
    Telefono varchar(50) NULL,
    Estado bit NOT NULL DEFAULT 1,
    FechaRegistro datetime NOT NULL DEFAULT GETDATE()
)
GO
CREATE UNIQUE INDEX UX_PROVEEDOR_Documento ON PROVEEDOR(Documento) WHERE Documento IS NOT NULL
GO

CREATE TABLE COMPRA(
    IdCompra int IDENTITY PRIMARY KEY,
    IdUsuario int NOT NULL REFERENCES USUARIO(IdUsuario),
    IdProveedor int NOT NULL REFERENCES PROVEEDOR(IdProveedor),
    TipoDocumento varchar(50) NOT NULL,
    NumeroDocumento varchar(50) NOT NULL,     -- COM-0001
    MetodoPago varchar(30) NOT NULL DEFAULT 'Efectivo',
    ReferenciaPago varchar(50) NULL,
    MontoTotal decimal(10,2) NOT NULL CHECK (MontoTotal >= 0),
    Anulada bit NOT NULL DEFAULT 0,
    FechaAnulacion datetime NULL,
    IdUsuarioAnula int NULL REFERENCES USUARIO(IdUsuario),
    MotivoAnulacion varchar(200) NULL,
    FechaRegistro datetime NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CK_COMPRA_Metodo CHECK (MetodoPago IN ('Efectivo', 'Tarjeta', 'Transferencia'))
)
GO
CREATE UNIQUE INDEX UX_COMPRA_Numero ON COMPRA(NumeroDocumento)
GO

CREATE TABLE DETALLE_COMPRA(
    IdDetalleCompra int IDENTITY PRIMARY KEY,
    IdCompra int NOT NULL REFERENCES COMPRA(IdCompra),
    IdProducto int NOT NULL REFERENCES PRODUCTO(IdProducto),
    PrecioCompra decimal(10,2) NOT NULL CHECK (PrecioCompra >= 0),
    PrecioVenta decimal(10,2) NOT NULL CHECK (PrecioVenta >= 0),
    Cantidad int NOT NULL CHECK (Cantidad > 0),
    MontoTotal decimal(10,2) NOT NULL,
    FechaRegistro datetime NOT NULL DEFAULT GETDATE()
)
GO

CREATE TABLE VENTA(
    IdVenta int IDENTITY PRIMARY KEY,
    IdUsuario int NOT NULL REFERENCES USUARIO(IdUsuario),
    IdCliente int NULL REFERENCES CLIENTE(IdCliente),   -- NULL = público en general
    TipoDocumento varchar(50) NOT NULL,
    NumeroDocumento varchar(50) NOT NULL,     -- VEN-0001
    MetodoPago varchar(30) NOT NULL DEFAULT 'Efectivo',
    ReferenciaPago varchar(50) NULL,
    Subtotal decimal(10,2) NOT NULL DEFAULT 0,
    DescuentoPorcentaje decimal(5,2) NOT NULL DEFAULT 0,
    MontoDescuento decimal(10,2) NOT NULL DEFAULT 0,
    MontoPago decimal(10,2) NOT NULL,
    MontoCambio decimal(10,2) NOT NULL,
    MontoTotal decimal(10,2) NOT NULL CHECK (MontoTotal >= 0),
    Anulada bit NOT NULL DEFAULT 0,
    FechaAnulacion datetime NULL,
    IdUsuarioAnula int NULL REFERENCES USUARIO(IdUsuario),
    MotivoAnulacion varchar(200) NULL,
    FechaRegistro datetime NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_VENTA_DOC UNIQUE (TipoDocumento, NumeroDocumento),
    CONSTRAINT CK_VENTA_Metodo CHECK (MetodoPago IN ('Efectivo', 'Tarjeta', 'Transferencia')),
    CONSTRAINT CK_VENTA_Descuento CHECK (DescuentoPorcentaje BETWEEN 0 AND 100)
)
GO

CREATE TABLE DETALLE_VENTA(
    IdDetalleVenta int IDENTITY PRIMARY KEY,
    IdVenta int NOT NULL REFERENCES VENTA(IdVenta),
    IdProducto int NOT NULL REFERENCES PRODUCTO(IdProducto),
    PrecioVenta decimal(10,2) NOT NULL CHECK (PrecioVenta >= 0),
    Cantidad int NOT NULL CHECK (Cantidad > 0),
    SubTotal decimal(10,2) NOT NULL,
    FechaRegistro datetime NOT NULL DEFAULT GETDATE()
)
GO

CREATE TABLE AJUSTE_INVENTARIO(
    IdAjuste int IDENTITY PRIMARY KEY,
    IdProducto int NOT NULL REFERENCES PRODUCTO(IdProducto),
    IdUsuario int NOT NULL REFERENCES USUARIO(IdUsuario),
    StockAnterior int NOT NULL,
    StockNuevo int NOT NULL CHECK (StockNuevo >= 0),
    Diferencia AS (StockNuevo - StockAnterior) PERSISTED,
    Motivo varchar(30) NOT NULL,
    Nota varchar(200) NULL,
    FechaRegistro datetime NOT NULL DEFAULT GETDATE()
)
GO

-- Último número usado de cada numeración automática
CREATE TABLE CONTADOR(
    Nombre varchar(20) PRIMARY KEY,
    Valor int NOT NULL
)
GO

/* ====================== DATOS DE CONFIGURACIÓN ====================== */
/* No son datos del negocio: sin ellos el programa no puede arrancar.   */

INSERT INTO ROL(Descripcion) VALUES ('ADMINISTRADOR'), ('EMPLEADO');
GO

INSERT INTO PERMISO(IdRol, NombreMenu)
SELECT r.IdRol, p.NombreMenu
FROM ROL r
INNER JOIN (VALUES
    ('ADMINISTRADOR', 'menuusuarios'),
    ('ADMINISTRADOR', 'menugestion'),
    ('ADMINISTRADOR', 'menuventas'),
    ('ADMINISTRADOR', 'menucompras'),
    ('ADMINISTRADOR', 'menuclientes'),
    ('ADMINISTRADOR', 'menuproveedor'),
    ('ADMINISTRADOR', 'menureportes'),
    ('ADMINISTRADOR', 'menuacercade'),
    ('ADMINISTRADOR', 'menurespaldo'),
    ('ADMINISTRADOR', 'anular'),
    ('EMPLEADO',      'menuventas'),
    ('EMPLEADO',      'menuclientes'),
    ('EMPLEADO',      'menuacercade')
) AS p(Rol, NombreMenu) ON p.Rol = r.Descripcion;
GO

INSERT INTO CONTADOR(Nombre, Valor) VALUES
    ('PRODUCTO', 0), ('SERVICIO', 0), ('CLIENTE', 0),
    ('PROVEEDOR', 0), ('COMPRA', 0), ('VENTA', 0);
GO

/* ===================== TIPOS DE TABLA (líneas de compra y venta) ===================== */

CREATE TYPE EDetalle_Compra AS TABLE(
    IdProducto int NOT NULL,
    PrecioCompra decimal(10,2) NOT NULL,
    PrecioVenta decimal(10,2) NOT NULL,
    Cantidad int NOT NULL
)
GO

CREATE TYPE EDetalle_Venta AS TABLE(
    IdProducto int NOT NULL,
    PrecioVenta decimal(10,2) NOT NULL,
    Cantidad int NOT NULL
)
GO

/* ================= PROCEDIMIENTOS: ACCESO Y USUARIOS ================= */

-- Datos del usuario para validar el inicio de sesión (la clave se verifica en el programa)
CREATE PROCEDURE SP_OBTENER_LOGIN(@NameUser varchar(50))
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

-- 5 fallos seguidos = bloqueo de 5 minutos
CREATE PROCEDURE SP_REGISTRAR_INTENTO_FALLIDO(@IdUsuario int)
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

CREATE PROCEDURE SP_LOGIN_EXITOSO(@IdUsuario int)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE USUARIO SET IntentosFallidos = 0, BloqueadoHasta = NULL
    WHERE IdUsuario = @IdUsuario
END
GO

CREATE PROCEDURE SP_EXISTE_USUARIOS
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM USUARIO
END
GO

CREATE PROCEDURE SP_LISTAR_ROLES
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdRol, Descripcion FROM ROL ORDER BY IdRol
END
GO

CREATE PROCEDURE SP_LISTAR_PERMISOS(@IdRol int)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT NombreMenu FROM PERMISO WHERE IdRol = @IdRol
END
GO

CREATE PROCEDURE SP_CREAR_USUARIO(
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

CREATE PROCEDURE SP_LISTAR_USUARIOS
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

CREATE PROCEDURE SP_EDITAR_USUARIO(
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

-- Cambiar la contraseña también limpia los intentos fallidos y el bloqueo
CREATE PROCEDURE SP_CAMBIAR_CLAVE(
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

CREATE PROCEDURE SP_DESBLOQUEAR_USUARIO(@IdUsuario int)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE USUARIO SET IntentosFallidos = 0, BloqueadoHasta = NULL
    WHERE IdUsuario = @IdUsuario;
END
GO

/* ===================== PROCEDIMIENTOS: CATEGORÍAS ===================== */

CREATE PROCEDURE SP_LISTAR_CATEGORIAS
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdCategoria, Descripcion, Estado
    FROM CATEGORIA
    ORDER BY Descripcion
END
GO

CREATE PROCEDURE SP_GUARDAR_CATEGORIA(
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

/* ============== PROCEDIMIENTOS: PRODUCTOS Y SERVICIOS ============== */

CREATE PROCEDURE SP_LISTAR_PRODUCTOS
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

CREATE PROCEDURE SP_GUARDAR_PRODUCTO(
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

/* ================== PROCEDIMIENTOS: CLIENTES ================== */

CREATE PROCEDURE SP_LISTAR_CLIENTES
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdCliente, Documento, NombreCompleto, Correo, Telefono, Estado
    FROM CLIENTE
    ORDER BY NombreCompleto
END
GO

CREATE PROCEDURE SP_GUARDAR_CLIENTE(
    @IdCliente int,                -- 0 = cliente nuevo
    @NombreCompleto varchar(100),
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

        INSERT INTO CLIENTE(Documento, NombreCompleto, Correo, Telefono, Estado)
        VALUES(@codigo, @NombreCompleto, @Correo, @Telefono, @Estado);
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
        SET NombreCompleto = @NombreCompleto, Correo = @Correo,
            Telefono = @Telefono, Estado = @Estado
        WHERE IdCliente = @IdCliente;
        SET @IdGenerado = @IdCliente;
    END
END
GO

/* ================== PROCEDIMIENTOS: PROVEEDORES ================== */

CREATE PROCEDURE SP_LISTAR_PROVEEDORES
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdProveedor, Documento, RazonSocial, Correo, Telefono, Estado
    FROM PROVEEDOR
    ORDER BY RazonSocial
END
GO

CREATE PROCEDURE SP_GUARDAR_PROVEEDOR(
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

/* ===================== PROCEDIMIENTOS: COMPRAS ===================== */

CREATE PROCEDURE SP_REGISTRAR_COMPRA(
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

CREATE PROCEDURE SP_LISTAR_COMPRAS(@Desde date, @Hasta date)
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

CREATE PROCEDURE SP_DETALLE_COMPRA(@IdCompra int)
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

/* ====================== PROCEDIMIENTOS: VENTAS ====================== */

CREATE PROCEDURE SP_REGISTRAR_VENTA(
    @IdUsuario int,
    @IdCliente int,                        -- NULL = público en general
    @MetodoPago varchar(30),
    @ReferenciaPago varchar(50),
    @DescuentoPorcentaje decimal(5,2),
    @MontoPago decimal(10,2),              -- solo se usa con Efectivo
    @Detalle EDetalle_Venta READONLY,
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
    SET @ReferenciaPago = NULLIF(LTRIM(RTRIM(ISNULL(@ReferenciaPago, ''))), '');

    -- ===== Validaciones previas =====
    IF NOT EXISTS (SELECT 1 FROM USUARIO WHERE IdUsuario = @IdUsuario AND Estado = 1)
    BEGIN SET @Mensaje = 'El usuario no es válido.'; RETURN; END

    IF @IdCliente IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM CLIENTE WHERE IdCliente = @IdCliente AND Estado = 1)
    BEGIN SET @Mensaje = 'El cliente no existe o está inactivo.'; RETURN; END

    IF ISNULL(@MetodoPago, '') NOT IN ('Efectivo', 'Tarjeta', 'Transferencia')
    BEGIN SET @Mensaje = 'El método de pago no es válido.'; RETURN; END

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

    DECLARE @n int, @numero varchar(50), @idVenta int, @filas int, @lineas int, @texto varchar(300);
    DECLARE @subtotal decimal(18,2), @descuento decimal(18,2), @total decimal(18,2);
    DECLARE @pago decimal(10,2), @cambio decimal(10,2);

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

        IF @MetodoPago = 'Efectivo'
        BEGIN
            IF @MontoPago IS NULL OR @MontoPago < @total
            BEGIN
                ROLLBACK;
                SET @Mensaje = 'El pago es menor al total.';
                RETURN;
            END
            SET @pago = @MontoPago;
            SET @cambio = @MontoPago - @total;
        END
        ELSE
        BEGIN
            SET @pago = @total;
            SET @cambio = 0;
        END

        -- 4) Número automático
        WHILE 1 = 1
        BEGIN
            UPDATE CONTADOR SET @n = Valor = Valor + 1 WHERE Nombre = 'VENTA';
            SET @numero = 'VEN-' + FORMAT(@n, '0000');
            IF NOT EXISTS (SELECT 1 FROM VENTA WHERE TipoDocumento = 'Ticket' AND NumeroDocumento = @numero) BREAK;
        END

        -- 5) Guardar la venta y su detalle
        INSERT INTO VENTA(IdUsuario, IdCliente, TipoDocumento, NumeroDocumento, MontoPago, MontoCambio,
                          MontoTotal, MetodoPago, ReferenciaPago, Subtotal, DescuentoPorcentaje, MontoDescuento)
        VALUES(@IdUsuario, @IdCliente, 'Ticket', @numero, @pago, @cambio,
               @total, @MetodoPago, @ReferenciaPago, @subtotal, @DescuentoPorcentaje, @descuento);
        SET @idVenta = SCOPE_IDENTITY();

        INSERT INTO DETALLE_VENTA(IdVenta, IdProducto, PrecioVenta, Cantidad, SubTotal)
        SELECT @idVenta, IdProducto, PrecioVenta, Cantidad, Cantidad * PrecioVenta
        FROM @Detalle;

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

CREATE PROCEDURE SP_LISTAR_VENTAS(@Desde date, @Hasta date, @IdUsuario int = NULL)
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

CREATE PROCEDURE SP_DETALLE_VENTA(@IdVenta int, @IdUsuario int = NULL)
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

/* ===================== PROCEDIMIENTOS: ANULACIONES ===================== */

CREATE PROCEDURE SP_ANULAR_VENTA(
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

CREATE PROCEDURE SP_ANULAR_COMPRA(
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

/* ================= PROCEDIMIENTOS: AJUSTE DE INVENTARIO ================= */

CREATE PROCEDURE SP_AJUSTAR_INVENTARIO(
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

CREATE PROCEDURE SP_LISTAR_AJUSTES(@Desde date, @Hasta date)
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

/* ========================= COMPROBACIÓN FINAL ========================= */
/* Resultado esperado: 13 tablas, 29 procedimientos, 2 tipos de tabla,    */
/* 2 roles, 13 permisos, 6 contadores y 0 filas de datos del negocio.     */

SELECT
    DB_NAME() AS BaseDeDatos,
    (SELECT COUNT(*) FROM sys.tables) AS Tablas,
    (SELECT COUNT(*) FROM sys.procedures) AS Procedimientos,
    (SELECT COUNT(*) FROM sys.types WHERE is_table_type = 1) AS TiposDeTabla,
    (SELECT COUNT(*) FROM ROL) AS Roles,
    (SELECT COUNT(*) FROM PERMISO) AS Permisos,
    (SELECT COUNT(*) FROM CONTADOR) AS Contadores,
    (SELECT COUNT(*) FROM USUARIO) + (SELECT COUNT(*) FROM CATEGORIA)
      + (SELECT COUNT(*) FROM PRODUCTO) + (SELECT COUNT(*) FROM CLIENTE)
      + (SELECT COUNT(*) FROM PROVEEDOR) + (SELECT COUNT(*) FROM COMPRA)
      + (SELECT COUNT(*) FROM VENTA) + (SELECT COUNT(*) FROM AJUSTE_INVENTARIO) AS FilasDeNegocio
GO

PRINT N'Base de datos creada correctamente. Abre el programa: pedirá crear al administrador.';
GO

SET NOEXEC OFF   -- vuelve a ejecutar normalmente (solo importa si la base ya existía)
GO
