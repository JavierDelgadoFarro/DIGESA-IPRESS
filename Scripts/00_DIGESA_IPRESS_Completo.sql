-- =============================================
-- DIGESA-IPRESS - Script Único de Base de Datos
-- Fecha: 2026-03-03
-- Descripción: Creación completa de tablas, estructura y datos iniciales
--              para el sistema DIGESA-IPRESS (datos independientes)
-- =============================================

SET NOCOUNT ON;
GO

-- =============================================
-- 1. TABLAS ADMINISTRATIVAS (ADM)
-- =============================================

-- ADM_Area
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ADM_Area')
BEGIN
    CREATE TABLE ADM_Area (
        ID_AREA INT PRIMARY KEY IDENTITY(1,1),
        DescripcionARE VARCHAR(100) NULL,
        AbreviaturasARE VARCHAR(20) NULL,
        ID_DIRECCION INT NULL,
        ID_ESTADO INT NULL DEFAULT 1,
        ID_PERSONAL INT NULL,
        JefaturasARE VARCHAR(1) NULL
    );
    PRINT 'Tabla ADM_Area creada.';
END
GO

-- ADM_Modulo
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ADM_Modulo')
BEGIN
    CREATE TABLE ADM_Modulo (
        ID_MODULO INT PRIMARY KEY IDENTITY(1,1),
        DescripcionMOD VARCHAR(60) NULL,
        Identificador VARCHAR(50) NULL,
        ID_ESTADO INT NULL DEFAULT 1
    );
    PRINT 'Tabla ADM_Modulo creada.';
END
GO

-- ADM_Menu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ADM_Menu')
BEGIN
    CREATE TABLE ADM_Menu (
        ID_MENU INT PRIMARY KEY IDENTITY(1,1),
        DescripcionMEN VARCHAR(100) NULL,
        ID_MODULO INT NOT NULL,
        ID_ESTADO INT NULL DEFAULT 1,
        OrdenMEN INT NULL DEFAULT 0,
        CONSTRAINT FK_ADM_Menu_ADM_Modulo FOREIGN KEY (ID_MODULO) REFERENCES ADM_Modulo(ID_MODULO)
    );
    PRINT 'Tabla ADM_Menu creada.';
END
GO

-- ADM_Sub_Menu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ADM_Sub_Menu')
BEGIN
    CREATE TABLE ADM_Sub_Menu (
        ID_SMENU INT PRIMARY KEY IDENTITY(1,1),
        DescripcionSME VARCHAR(200) NULL,
        RutaWebSME VARCHAR(100) NULL,
        ID_MENU INT NOT NULL,
        ID_ESTADO INT NULL DEFAULT 1,
        OrdenSME INT NULL DEFAULT 0,
        CONSTRAINT FK_ADM_Sub_Menu_ADM_Menu FOREIGN KEY (ID_MENU) REFERENCES ADM_Menu(ID_MENU)
    );
    PRINT 'Tabla ADM_Sub_Menu creada.';
END
GO

-- ADM_Personal
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ADM_Personal')
BEGIN
    CREATE TABLE ADM_Personal (
        ID_PERSONAL INT PRIMARY KEY IDENTITY(1,1),
        ApellidosNombrePER VARCHAR(100) NULL,
        Nombre VARCHAR(100) NULL,
        Paterno VARCHAR(100) NULL,
        Materno VARCHAR(100) NULL,
        DNIPER VARCHAR(50) NULL,
        ID_AREA INT NULL,
        ID_ESTADO INT NULL DEFAULT 1,
        EmailPER VARCHAR(400) NULL,
        CONSTRAINT FK_Personal_Area FOREIGN KEY (ID_AREA) REFERENCES ADM_Area(ID_AREA)
    );
    PRINT 'Tabla ADM_Personal creada.';
END
GO

-- ADM_Usuario
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ADM_Usuario')
BEGIN
    CREATE TABLE ADM_Usuario (
        ID_USUARIO INT PRIMARY KEY IDENTITY(1,1),
        NombreUsuarioUSU VARCHAR(20) NOT NULL,
        ContrasenaUSU VARCHAR(100) NOT NULL,
        ID_PERSONAL INT NULL,
        ID_AREA INT NULL,
        ID_SEDE INT NULL,
        ID_ESTADO INT NULL DEFAULT 1,
        CONSTRAINT FK_Usuario_Personal FOREIGN KEY (ID_PERSONAL) REFERENCES ADM_Personal(ID_PERSONAL),
        CONSTRAINT UQ_ADM_Usuario_Nombre UNIQUE (NombreUsuarioUSU)
    );
    PRINT 'Tabla ADM_Usuario creada.';
END
GO

-- ADM_Detalle_Sub_Menu (permisos usuario-submenu)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ADM_Detalle_Sub_Menu')
BEGIN
    CREATE TABLE ADM_Detalle_Sub_Menu (
        ID_DSUBMENU INT PRIMARY KEY IDENTITY(1,1),
        ID_SMENU INT NOT NULL,
        ID_USUARIO INT NOT NULL,
        CONSTRAINT FK_ADM_Detalle_Sub_Menu_ADM_Sub_Menu FOREIGN KEY (ID_SMENU) REFERENCES ADM_Sub_Menu(ID_SMENU),
        CONSTRAINT FK_ADM_Detalle_Sub_Menu_ADM_Usuario FOREIGN KEY (ID_USUARIO) REFERENCES ADM_Usuario(ID_USUARIO)
    );
    PRINT 'Tabla ADM_Detalle_Sub_Menu creada.';
END
GO

-- =============================================
-- 2. DATOS INICIALES (SEED)
-- =============================================

-- Área por defecto
IF NOT EXISTS (SELECT 1 FROM ADM_Area)
BEGIN
    INSERT INTO ADM_Area (DescripcionARE, AbreviaturasARE, ID_ESTADO) 
    VALUES ('Área General', 'AG', 1);
    PRINT 'Área inicial insertada.';
END
GO

-- Personal admin
IF NOT EXISTS (SELECT 1 FROM ADM_Personal)
BEGIN
    DECLARE @IdArea INT = (SELECT TOP 1 ID_AREA FROM ADM_Area);
    INSERT INTO ADM_Personal (ApellidosNombrePER, Nombre, Paterno, Materno, ID_AREA, ID_ESTADO)
    VALUES ('Administrador Sistema', 'Administrador', 'Sistema', NULL, @IdArea, 1);
    PRINT 'Personal administrador insertado.';
END
GO

-- Usuario admin (usuario: admin, contraseña: admin - cambiar en primer login)
IF NOT EXISTS (SELECT 1 FROM ADM_Usuario)
BEGIN
    DECLARE @IdPersonal INT = (SELECT TOP 1 ID_PERSONAL FROM ADM_Personal);
    DECLARE @IdAreaUsu INT = (SELECT TOP 1 ID_AREA FROM ADM_Area);
    INSERT INTO ADM_Usuario (NombreUsuarioUSU, ContrasenaUSU, ID_PERSONAL, ID_AREA, ID_ESTADO)
    VALUES ('admin', 'admin', @IdPersonal, @IdAreaUsu, 1);
    PRINT 'Usuario admin insertado (usuario: admin, contraseña: admin).';
END
GO

-- Módulo, Menú y SubMenú para Principal
IF NOT EXISTS (SELECT 1 FROM ADM_Modulo)
BEGIN
    INSERT INTO ADM_Modulo (DescripcionMOD, Identificador, ID_ESTADO) 
    VALUES ('DIGESA-IPRESS', 'DIGESA_IPRESS', 1);
    
    INSERT INTO ADM_Menu (DescripcionMEN, ID_MODULO, ID_ESTADO, OrdenMEN)
    VALUES ('Principal', (SELECT TOP 1 ID_MODULO FROM ADM_Modulo), 1, 1);
    
    INSERT INTO ADM_Sub_Menu (DescripcionSME, RutaWebSME, ID_MENU, ID_ESTADO, OrdenSME)
    VALUES ('Inicio', '/principal', (SELECT TOP 1 ID_MENU FROM ADM_Menu), 1, 1);
    
    -- Asignar acceso al usuario admin
    INSERT INTO ADM_Detalle_Sub_Menu (ID_SMENU, ID_USUARIO)
    SELECT s.ID_SMENU, u.ID_USUARIO 
    FROM ADM_Sub_Menu s, ADM_Usuario u 
    WHERE s.RutaWebSME = '/principal' AND u.NombreUsuarioUSU = 'admin'
    AND NOT EXISTS (SELECT 1 FROM ADM_Detalle_Sub_Menu d WHERE d.ID_SMENU = s.ID_SMENU AND d.ID_USUARIO = u.ID_USUARIO);
    
    PRINT 'Módulo, menú y permisos iniciales insertados.';
END
GO

-- =============================================
-- 3. PROCEDIMIENTOS ALMACENADOS (opcionales)
-- =============================================

-- SP para validar login (opcional, el API puede validar directamente)
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_ValidarUsuario')
    DROP PROCEDURE SP_ValidarUsuario;
GO

CREATE PROCEDURE SP_ValidarUsuario
    @Usuario VARCHAR(20),
    @Contrasena VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.ID_USUARIO, u.NombreUsuarioUSU, p.ApellidosNombrePER
    FROM ADM_Usuario u
    LEFT JOIN ADM_Personal p ON u.ID_PERSONAL = p.ID_PERSONAL
    WHERE u.NombreUsuarioUSU = @Usuario AND u.ContrasenaUSU = @Contrasena AND u.ID_ESTADO = 1;
END
GO

PRINT 'Script DIGESA-IPRESS ejecutado correctamente.';
PRINT 'Credenciales iniciales: usuario=admin, contraseña=admin (cambiar en primer acceso)';
GO
