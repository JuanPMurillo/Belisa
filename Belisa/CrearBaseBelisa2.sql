/* =====================================================================
   COMPLEMENTO PARA DBBELISA
   Ejecutar DESPUES de CrearBaseBelisa.sql, sobre la base nueva.

   Recupera los datos de configuración que sí incluía el script antiguo:
     - Roles ADMINISTRADOR y EMPLEADO
     - Permisos de menú para ambos roles
     - Contadores de numeración

   Es idempotente: no duplica roles, permisos ni contadores existentes,
   y no reinicia valores de contadores que ya estén en uso.

   IMPORTANTE:
   - Haz respaldo antes de ejecutarlo.
   - No elimina ni reemplaza tablas, procedimientos o datos del negocio.
   - Este complemento no crea usuarios; el programa debe permitir crear
     el primer administrador.
   ===================================================================== */

USE [DBBELISA];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.ROL', N'U') IS NULL
   OR OBJECT_ID(N'dbo.PERMISO', N'U') IS NULL
   OR OBJECT_ID(N'dbo.CONTADOR', N'U') IS NULL
BEGIN
    THROW 50001, 'Faltan las tablas ROL, PERMISO o CONTADOR. Ejecuta primero CrearBaseBelisa.sql en la base DBBELISA.', 1;
END;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    /* 1. Roles necesarios para permisos e inicio de sesión */
    IF NOT EXISTS (SELECT 1 FROM dbo.ROL WHERE Descripcion = 'ADMINISTRADOR')
        INSERT INTO dbo.ROL (Descripcion) VALUES ('ADMINISTRADOR');

    IF NOT EXISTS (SELECT 1 FROM dbo.ROL WHERE Descripcion = 'EMPLEADO')
        INSERT INTO dbo.ROL (Descripcion) VALUES ('EMPLEADO');

    /* 2. Permisos de menú del script antiguo.
          Los permisos ya existentes se respetan. */
    DECLARE @Permisos TABLE
    (
        Rol varchar(50) NOT NULL,
        NombreMenu varchar(100) NOT NULL,
        PRIMARY KEY (Rol, NombreMenu)
    );

    INSERT INTO @Permisos (Rol, NombreMenu) VALUES
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
        ('EMPLEADO',      'menuacercade');

    INSERT INTO dbo.PERMISO (IdRol, NombreMenu)
    SELECT r.IdRol, p.NombreMenu
    FROM @Permisos AS p
    INNER JOIN dbo.ROL AS r ON r.Descripcion = p.Rol
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.PERMISO AS existente
        WHERE existente.IdRol = r.IdRol
          AND existente.NombreMenu = p.NombreMenu
    );

    /* 3. Contadores de códigos automáticos.
          No se sobrescribe Valor si el contador ya existe. */
    DECLARE @Contadores TABLE
    (
        Nombre varchar(20) NOT NULL PRIMARY KEY,
        Valor int NOT NULL
    );

    INSERT INTO @Contadores (Nombre, Valor) VALUES
        ('PRODUCTO', 0),
        ('SERVICIO', 0),
        ('CLIENTE', 0),
        ('PROVEEDOR', 0),
        ('COMPRA', 0),
        ('VENTA', 0);

    INSERT INTO dbo.CONTADOR (Nombre, Valor)
    SELECT c.Nombre, c.Valor
    FROM @Contadores AS c
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.CONTADOR AS existente
        WHERE existente.Nombre = c.Nombre
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

/* 4. Verificación de lo que quedó configurado */
PRINT 'Roles disponibles:';
SELECT IdRol, Descripcion FROM dbo.ROL ORDER BY Descripcion;

PRINT 'Permisos configurados:';
SELECT r.Descripcion AS Rol, p.NombreMenu
FROM dbo.PERMISO AS p
INNER JOIN dbo.ROL AS r ON r.IdRol = p.IdRol
ORDER BY r.Descripcion, p.NombreMenu;

PRINT 'Contadores disponibles:';
SELECT Nombre, Valor FROM dbo.CONTADOR ORDER BY Nombre;
GO
