-- =============================================
-- IPRESS - SCRIPT ÚNICO DDL (ESTRUCTURA DE BASE DE DATOS)
-- =============================================
-- Incluye: creación de BD, tablas, índices, procedimientos almacenados,
-- funciones, triggers y migraciones de columnas.
-- NO incluye INSERT ni limpieza de datos (ver IPRESS_Inserts.sql e IPRESS_Limpieza.sql).
--
-- Orden de ejecución recomendado: 1) IPRESS_DDL.sql  2) IPRESS_Inserts.sql
-- =============================================

SET NOCOUNT ON;
GO

-- =============================================
-- CREAR BASE DE DATOS
-- =============================================
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'IPRESS')
    CREATE DATABASE IPRESS;
GO

USE IPRESS;
GO

-- =============================================
-- 0. ELIMINAR TABLAS LEGACY ADM_ (NO USADAS; EL PROYECTO USA SOLO IPRESS_)
-- =============================================
-- Eliminar FKs que apunten a tablas ADM_ para poder borrarlas en cualquier orden
DECLARE @sql NVARCHAR(500);
DECLARE cr CURSOR LOCAL FAST_FORWARD FOR
    SELECT 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + ' DROP CONSTRAINT ' + QUOTENAME(name)
    FROM sys.foreign_keys
    WHERE referenced_object_id IN (OBJECT_ID('ADM_Area'), OBJECT_ID('ADM_Detalle_Sub_Menu'), OBJECT_ID('ADM_Menu'), OBJECT_ID('ADM_Modulo'), OBJECT_ID('ADM_Personal'), OBJECT_ID('ADM_Sub_Menu'), OBJECT_ID('ADM_Usuario'));
OPEN cr;
FETCH NEXT FROM cr INTO @sql;
WHILE @@FETCH_STATUS = 0 BEGIN EXEC sp_executesql @sql; FETCH NEXT FROM cr INTO @sql; END;
CLOSE cr; DEALLOCATE cr;
-- Eliminar FKs definidas en tablas ADM_ que referencian a otras
DECLARE cr2 CURSOR LOCAL FAST_FORWARD FOR
    SELECT 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + ' DROP CONSTRAINT ' + QUOTENAME(name)
    FROM sys.foreign_keys
    WHERE parent_object_id IN (OBJECT_ID('ADM_Area'), OBJECT_ID('ADM_Detalle_Sub_Menu'), OBJECT_ID('ADM_Menu'), OBJECT_ID('ADM_Modulo'), OBJECT_ID('ADM_Personal'), OBJECT_ID('ADM_Sub_Menu'), OBJECT_ID('ADM_Usuario'));
OPEN cr2;
FETCH NEXT FROM cr2 INTO @sql;
WHILE @@FETCH_STATUS = 0 BEGIN EXEC sp_executesql @sql; FETCH NEXT FROM cr2 INTO @sql; END;
CLOSE cr2; DEALLOCATE cr2;
-- Borrar tablas ADM_
IF OBJECT_ID('dbo.ADM_Detalle_Sub_Menu', 'U') IS NOT NULL DROP TABLE dbo.ADM_Detalle_Sub_Menu;
IF OBJECT_ID('dbo.ADM_Usuario', 'U') IS NOT NULL DROP TABLE dbo.ADM_Usuario;
IF OBJECT_ID('dbo.ADM_Sub_Menu', 'U') IS NOT NULL DROP TABLE dbo.ADM_Sub_Menu;
IF OBJECT_ID('dbo.ADM_Personal', 'U') IS NOT NULL DROP TABLE dbo.ADM_Personal;
IF OBJECT_ID('dbo.ADM_Menu', 'U') IS NOT NULL DROP TABLE dbo.ADM_Menu;
IF OBJECT_ID('dbo.ADM_Area', 'U') IS NOT NULL DROP TABLE dbo.ADM_Area;
IF OBJECT_ID('dbo.ADM_Modulo', 'U') IS NOT NULL DROP TABLE dbo.ADM_Modulo;
GO

-- =============================================
-- 1. SEGURIDAD: MÓDULOS, BOTONES, ROLES, PERMISOS
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Modulo')
CREATE TABLE IPRESS_Modulo (
    ID_MODULO INT PRIMARY KEY IDENTITY(1,1),
    Codigo VARCHAR(50) NOT NULL UNIQUE,
    Nombre VARCHAR(100) NOT NULL,
    Ruta VARCHAR(100) NULL,
    Descripcion VARCHAR(500) NULL,
    Orden INT DEFAULT 0
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Boton')
CREATE TABLE IPRESS_Boton (
    ID_BOTON INT PRIMARY KEY IDENTITY(1,1),
    ID_MODULO INT NOT NULL,
    Codigo VARCHAR(50) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    CONSTRAINT FK_Boton_Modulo FOREIGN KEY (ID_MODULO) REFERENCES IPRESS_Modulo(ID_MODULO)
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Rol')
CREATE TABLE IPRESS_Rol (
    ID_ROL INT PRIMARY KEY IDENTITY(1,1),
    Codigo VARCHAR(50) NOT NULL UNIQUE,
    Nombre VARCHAR(100) NOT NULL
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_RolModulo')
CREATE TABLE IPRESS_RolModulo (
    ID_ROL INT NOT NULL,
    ID_MODULO INT NOT NULL,
    PRIMARY KEY (ID_ROL, ID_MODULO),
    CONSTRAINT FK_RolModulo_Rol FOREIGN KEY (ID_ROL) REFERENCES IPRESS_Rol(ID_ROL),
    CONSTRAINT FK_RolModulo_Modulo FOREIGN KEY (ID_MODULO) REFERENCES IPRESS_Modulo(ID_MODULO)
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_RolBoton')
CREATE TABLE IPRESS_RolBoton (
    ID_ROL INT NOT NULL,
    ID_BOTON INT NOT NULL,
    PRIMARY KEY (ID_ROL, ID_BOTON),
    CONSTRAINT FK_RolBoton_Rol FOREIGN KEY (ID_ROL) REFERENCES IPRESS_Rol(ID_ROL),
    CONSTRAINT FK_RolBoton_Boton FOREIGN KEY (ID_BOTON) REFERENCES IPRESS_Boton(ID_BOTON)
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Usuario')
CREATE TABLE IPRESS_Usuario (
    ID_USUARIO INT PRIMARY KEY IDENTITY(1,1),
    NombreUsuario VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(200) NOT NULL,
    NombreCompleto VARCHAR(200) NOT NULL,
    Email VARCHAR(200) NULL,
    Activo BIT DEFAULT 1,
    FechaCreacion DATETIME DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_UsuarioRol')
CREATE TABLE IPRESS_UsuarioRol (
    ID_USUARIO INT NOT NULL,
    ID_ROL INT NOT NULL,
    PRIMARY KEY (ID_USUARIO, ID_ROL),
    CONSTRAINT FK_UsuarioRol_Usuario FOREIGN KEY (ID_USUARIO) REFERENCES IPRESS_Usuario(ID_USUARIO),
    CONSTRAINT FK_UsuarioRol_Rol FOREIGN KEY (ID_ROL) REFERENCES IPRESS_Rol(ID_ROL)
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Menu')
CREATE TABLE IPRESS_Menu (
    ID_MENU INT PRIMARY KEY IDENTITY(1,1),
    ID_MODULO INT NOT NULL,
    Codigo VARCHAR(50) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Orden INT DEFAULT 0,
    CONSTRAINT FK_Menu_Modulo FOREIGN KEY (ID_MODULO) REFERENCES IPRESS_Modulo(ID_MODULO)
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_SubMenu')
CREATE TABLE IPRESS_SubMenu (
    ID_SUBMENU INT PRIMARY KEY IDENTITY(1,1),
    ID_MENU INT NOT NULL,
    Codigo VARCHAR(50) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Ruta VARCHAR(200) NOT NULL,
    Icono VARCHAR(50) NULL,
    Orden INT DEFAULT 0,
    CONSTRAINT FK_SubMenu_Menu FOREIGN KEY (ID_MENU) REFERENCES IPRESS_Menu(ID_MENU)
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_RolSubMenu')
CREATE TABLE IPRESS_RolSubMenu (
    ID_ROL INT NOT NULL,
    ID_SUBMENU INT NOT NULL,
    PRIMARY KEY (ID_ROL, ID_SUBMENU),
    CONSTRAINT FK_RolSubMenu_Rol FOREIGN KEY (ID_ROL) REFERENCES IPRESS_Rol(ID_ROL),
    CONSTRAINT FK_RolSubMenu_SubMenu FOREIGN KEY (ID_SUBMENU) REFERENCES IPRESS_SubMenu(ID_SUBMENU)
);

-- =============================================
-- 2. GEOGRAFÍA PERÚ
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Departamento')
CREATE TABLE IPRESS_Departamento (
    Codigo CHAR(2) NOT NULL PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Provincia')
CREATE TABLE IPRESS_Provincia (
    Codigo CHAR(4) NOT NULL PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    CodigoDepartamento CHAR(2) NOT NULL,
    CONSTRAINT FK_Provincia_Departamento FOREIGN KEY (CodigoDepartamento) REFERENCES IPRESS_Departamento(Codigo)
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Distrito')
CREATE TABLE IPRESS_Distrito (
    Ubigeo CHAR(6) NOT NULL PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    CodigoProvincia CHAR(4) NOT NULL,
    CONSTRAINT FK_Distrito_Provincia FOREIGN KEY (CodigoProvincia) REFERENCES IPRESS_Provincia(Codigo)
);

-- =============================================
-- 3. MAESTROS: DIRESA, RED, MICRORED
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Diresa')
CREATE TABLE IPRESS_Diresa (
    ID_DIRESA INT PRIMARY KEY IDENTITY(1,1),
    Codigo INT NOT NULL UNIQUE,
    Nombre VARCHAR(200) NOT NULL,
    Ubigeo CHAR(6) NULL,
    Activo BIT DEFAULT 1,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Diresa_Distrito FOREIGN KEY (Ubigeo) REFERENCES IPRESS_Distrito(Ubigeo)
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Red')
CREATE TABLE IPRESS_Red (
    ID_RED INT PRIMARY KEY IDENTITY(1,1),
    ID_DIRESA INT NOT NULL,
    Codigo INT NOT NULL,
    Nombre VARCHAR(200) NOT NULL,
    Ubigeo CHAR(6) NULL,
    Activo BIT DEFAULT 1,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Red_Diresa FOREIGN KEY (ID_DIRESA) REFERENCES IPRESS_Diresa(ID_DIRESA),
    CONSTRAINT FK_Red_Distrito FOREIGN KEY (Ubigeo) REFERENCES IPRESS_Distrito(Ubigeo)
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_MicroRed')
CREATE TABLE IPRESS_MicroRed (
    ID_MICRORED INT PRIMARY KEY IDENTITY(1,1),
    ID_RED INT NOT NULL,
    Codigo INT NOT NULL,
    Nombre VARCHAR(200) NOT NULL,
    Ubigeo CHAR(6) NULL,
    Activo BIT DEFAULT 1,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_MicroRed_Red FOREIGN KEY (ID_RED) REFERENCES IPRESS_Red(ID_RED),
    CONSTRAINT FK_MicroRed_Distrito FOREIGN KEY (Ubigeo) REFERENCES IPRESS_Distrito(Ubigeo)
);

-- Migración: Ubigeo en Diresa/Red/MicroRed (si tablas existían sin Ubigeo)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Diresa')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Diresa') AND name = 'Ubigeo')
BEGIN
    ALTER TABLE IPRESS_Diresa ADD Ubigeo CHAR(6) NULL;
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Distrito')
        ALTER TABLE IPRESS_Diresa ADD CONSTRAINT FK_Diresa_Distrito FOREIGN KEY (Ubigeo) REFERENCES IPRESS_Distrito(Ubigeo);
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Red')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Red') AND name = 'Ubigeo')
BEGIN
    ALTER TABLE IPRESS_Red ADD Ubigeo CHAR(6) NULL;
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Distrito')
        ALTER TABLE IPRESS_Red ADD CONSTRAINT FK_Red_Distrito FOREIGN KEY (Ubigeo) REFERENCES IPRESS_Distrito(Ubigeo);
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_MicroRed')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_MicroRed') AND name = 'Ubigeo')
BEGIN
    ALTER TABLE IPRESS_MicroRed ADD Ubigeo CHAR(6) NULL;
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Distrito')
        ALTER TABLE IPRESS_MicroRed ADD CONSTRAINT FK_MicroRed_Distrito FOREIGN KEY (Ubigeo) REFERENCES IPRESS_Distrito(Ubigeo);
END
GO

-- Migración: Codigo VARCHAR -> INT en Diresa, Red, MicroRed
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Diresa') AND name = 'Codigo')
   AND (SELECT system_type_id FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Diresa') AND name = 'Codigo') IN (167, 231)
BEGIN
    DECLARE @uq_diresa NVARCHAR(256), @sql_diresa NVARCHAR(500);
    SELECT @uq_diresa = k.name FROM sys.key_constraints k
    INNER JOIN sys.index_columns ic ON k.parent_object_id = ic.object_id AND k.unique_index_id = ic.index_id
    INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    WHERE k.parent_object_id = OBJECT_ID('IPRESS_Diresa') AND k.type = 'UQ' AND c.name = 'Codigo';
    IF @uq_diresa IS NOT NULL BEGIN SET @sql_diresa = N'ALTER TABLE IPRESS_Diresa DROP CONSTRAINT ' + QUOTENAME(@uq_diresa); EXEC sp_executesql @sql_diresa; END
    ALTER TABLE IPRESS_Diresa ALTER COLUMN Codigo INT NOT NULL;
    ALTER TABLE IPRESS_Diresa ADD CONSTRAINT UQ_IPRESS_Diresa_Codigo UNIQUE (Codigo);
END
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Red') AND name = 'Codigo')
   AND (SELECT system_type_id FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Red') AND name = 'Codigo') IN (167, 231)
BEGIN
    DECLARE @uq_red NVARCHAR(256), @sql_red NVARCHAR(500);
    SELECT @uq_red = k.name FROM sys.key_constraints k
    INNER JOIN sys.index_columns ic ON k.parent_object_id = ic.object_id AND k.unique_index_id = ic.index_id
    INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    WHERE k.parent_object_id = OBJECT_ID('IPRESS_Red') AND k.type = 'UQ' AND c.name = 'Codigo';
    IF @uq_red IS NOT NULL BEGIN SET @sql_red = N'ALTER TABLE IPRESS_Red DROP CONSTRAINT ' + QUOTENAME(@uq_red); EXEC sp_executesql @sql_red; END
    ALTER TABLE IPRESS_Red ALTER COLUMN Codigo INT NOT NULL;
    ALTER TABLE IPRESS_Red ADD CONSTRAINT UQ_IPRESS_Red_Codigo UNIQUE (Codigo);
END
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_MicroRed') AND name = 'Codigo')
   AND (SELECT system_type_id FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_MicroRed') AND name = 'Codigo') IN (167, 231)
BEGIN
    DECLARE @uq_mr NVARCHAR(256), @sql_mr NVARCHAR(500);
    SELECT @uq_mr = k.name FROM sys.key_constraints k
    INNER JOIN sys.index_columns ic ON k.parent_object_id = ic.object_id AND k.unique_index_id = ic.index_id
    INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    WHERE k.parent_object_id = OBJECT_ID('IPRESS_MicroRed') AND k.type = 'UQ' AND c.name = 'Codigo';
    IF @uq_mr IS NOT NULL BEGIN SET @sql_mr = N'ALTER TABLE IPRESS_MicroRed DROP CONSTRAINT ' + QUOTENAME(@uq_mr); EXEC sp_executesql @sql_mr; END
    ALTER TABLE IPRESS_MicroRed ALTER COLUMN Codigo INT NOT NULL;
    ALTER TABLE IPRESS_MicroRed ADD CONSTRAINT UQ_IPRESS_MicroRed_Codigo UNIQUE (Codigo);
END
GO

-- Migración: Red - clave única por (ID_DIRESA, Codigo) para permitir mismo código en distintas Diresas
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Red')
BEGIN
    IF EXISTS (SELECT * FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('IPRESS_Red') AND name = 'UQ_IPRESS_Red_Codigo')
        ALTER TABLE IPRESS_Red DROP CONSTRAINT UQ_IPRESS_Red_Codigo;
    IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('IPRESS_Red') AND name = 'UQ_IPRESS_Red_Diresa_Codigo')
        ALTER TABLE IPRESS_Red ADD CONSTRAINT UQ_IPRESS_Red_Diresa_Codigo UNIQUE (ID_DIRESA, Codigo);
END
GO
-- Migración: MicroRed - clave única por (ID_RED, Codigo) para permitir mismo código en distintas Redes
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_MicroRed')
BEGIN
    IF EXISTS (SELECT * FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('IPRESS_MicroRed') AND name = 'UQ_IPRESS_MicroRed_Codigo')
        ALTER TABLE IPRESS_MicroRed DROP CONSTRAINT UQ_IPRESS_MicroRed_Codigo;
    IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('IPRESS_MicroRed') AND name = 'UQ_IPRESS_MicroRed_Red_Codigo')
        ALTER TABLE IPRESS_MicroRed ADD CONSTRAINT UQ_IPRESS_MicroRed_Red_Codigo UNIQUE (ID_RED, Codigo);
END
GO

-- Usuario: columna ID_DIRESA (Resto ID_RED, ID_MICRORED, ID_ESTABLECIMIENTO después de crear Establecimiento)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Usuario')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Usuario') AND name = 'ID_DIRESA')
BEGIN
    ALTER TABLE IPRESS_Usuario ADD ID_DIRESA INT NULL;
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Diresa')
        ALTER TABLE IPRESS_Usuario ADD CONSTRAINT FK_Usuario_Diresa FOREIGN KEY (ID_DIRESA) REFERENCES IPRESS_Diresa(ID_DIRESA);
END
GO

-- =============================================
-- 4. CENTROS POBLADOS (y tablas hijas)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_CentroPoblado')
CREATE TABLE IPRESS_CentroPoblado (
    ID_CENTRO_POBLADO INT PRIMARY KEY IDENTITY(1,1),
    UbigeoCcp VARCHAR(20) NOT NULL DEFAULT '',
    Departamento VARCHAR(100) NULL,
    Provincia VARCHAR(100) NULL,
    Distrito VARCHAR(100) NULL,
    CentroPoblado VARCHAR(200) NULL,
    Ambito VARCHAR(20) NULL,
    Quintil VARCHAR(5) NULL,
    Este DECIMAL(18,2) NULL,
    Norte DECIMAL(18,2) NULL,
    Huso INT NULL,
    Banda VARCHAR(5) NULL,
    Latitud DECIMAL(12,6) NULL,
    Longitud DECIMAL(12,6) NULL,
    AltitudMsnm INT NULL,
    PoblacionTotal INT NULL,
    PoblacionServida INT NULL,
    PoblacionVigilada INT NULL,
    Activo BIT DEFAULT 1
);

-- Columnas extendidas Centro Poblado (ID_ESTABLECIMIENTO se agrega después de crear Establecimiento)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_CentroPoblado')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'Ubigeo')
        ALTER TABLE IPRESS_CentroPoblado ADD Ubigeo VARCHAR(6) NULL;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'ElectricidadHrs')   ALTER TABLE IPRESS_CentroPoblado ADD ElectricidadHrs INT NULL;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'TelefonoTipo')     ALTER TABLE IPRESS_CentroPoblado ADD TelefonoTipo VARCHAR(100) NULL;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'TelefonoNumero')    ALTER TABLE IPRESS_CentroPoblado ADD TelefonoNumero VARCHAR(30) NULL;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'RadioEmisora')      ALTER TABLE IPRESS_CentroPoblado ADD RadioEmisora BIT DEFAULT 0;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'RadioESS')          ALTER TABLE IPRESS_CentroPoblado ADD RadioESS BIT DEFAULT 0;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'SenalTV')           ALTER TABLE IPRESS_CentroPoblado ADD SenalTV BIT DEFAULT 0;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'Internet')          ALTER TABLE IPRESS_CentroPoblado ADD Internet BIT DEFAULT 0;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'LimpiezaPublica')   ALTER TABLE IPRESS_CentroPoblado ADD LimpiezaPublica BIT DEFAULT 0;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'Agua')              ALTER TABLE IPRESS_CentroPoblado ADD Agua BIT DEFAULT 0;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'Letrinas')           ALTER TABLE IPRESS_CentroPoblado ADD Letrinas BIT DEFAULT 0;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'DesagueAlcantarillado') ALTER TABLE IPRESS_CentroPoblado ADD DesagueAlcantarillado BIT DEFAULT 0;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'SistEliminacionExcretas') ALTER TABLE IPRESS_CentroPoblado ADD SistEliminacionExcretas BIT DEFAULT 0;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'Vertimientos')      ALTER TABLE IPRESS_CentroPoblado ADD Vertimientos VARCHAR(100) NULL;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'TempMinima')        ALTER TABLE IPRESS_CentroPoblado ADD TempMinima DECIMAL(6,2) NULL;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'TempMaxima')        ALTER TABLE IPRESS_CentroPoblado ADD TempMaxima DECIMAL(6,2) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_CentroPobladoAccesibilidad')
CREATE TABLE IPRESS_CentroPobladoAccesibilidad (
    ID_ACCESIBILIDAD INT PRIMARY KEY IDENTITY(1,1),
    ID_CENTRO_POBLADO INT NOT NULL,
    Desde VARCHAR(100) NULL,
    Hasta VARCHAR(100) NULL,
    DistanciaKm DECIMAL(10,2) NULL,
    TiempoMin INT NULL,
    TipoVia VARCHAR(100) NULL,
    MedioTransporte VARCHAR(100) NULL,
    CONSTRAINT FK_Accesibilidad_CP FOREIGN KEY (ID_CENTRO_POBLADO) REFERENCES IPRESS_CentroPoblado(ID_CENTRO_POBLADO) ON DELETE CASCADE
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_CentroPobladoCentroEducativo')
CREATE TABLE IPRESS_CentroPobladoCentroEducativo (
    ID_CENTRO_EDUCATIVO INT PRIMARY KEY IDENTITY(1,1),
    ID_CENTRO_POBLADO INT NOT NULL,
    TipoCentroEducativo VARCHAR(50) NULL,
    NombreCentroEducativo VARCHAR(300) NULL,
    CONSTRAINT FK_CE_CP FOREIGN KEY (ID_CENTRO_POBLADO) REFERENCES IPRESS_CentroPoblado(ID_CENTRO_POBLADO) ON DELETE CASCADE
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_CentroPobladoAutoridad')
CREATE TABLE IPRESS_CentroPobladoAutoridad (
    ID_AUTORIDAD INT PRIMARY KEY IDENTITY(1,1),
    ID_CENTRO_POBLADO INT NOT NULL,
    TipoAutoridad VARCHAR(100) NULL,
    NombreAutoridad VARCHAR(200) NULL,
    CONSTRAINT FK_Aut_CP FOREIGN KEY (ID_CENTRO_POBLADO) REFERENCES IPRESS_CentroPoblado(ID_CENTRO_POBLADO) ON DELETE CASCADE
);
GO

-- =============================================
-- 5. ESTABLECIMIENTOS DE SALUD
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Establecimiento')
CREATE TABLE IPRESS_Establecimiento (
    ID_ESTABLECIMIENTO INT PRIMARY KEY IDENTITY(1,1),
    Codigo VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(300) NOT NULL,
    Departamento VARCHAR(100) NULL,
    Provincia VARCHAR(100) NULL,
    Distrito VARCHAR(100) NULL,
    QuintilRegional VARCHAR(5) NULL,
    Ubigeo VARCHAR(10) NULL,
    AltitudMsnm INT NULL,
    ID_DIRESA INT NULL,
    ID_RED INT NULL,
    ID_MICRORED INT NULL,
    TieneTelefono BIT DEFAULT 0,
    TieneRadio BIT DEFAULT 0,
    Este DECIMAL(18,2) NULL,
    Norte DECIMAL(18,2) NULL,
    Huso INT NULL,
    Banda VARCHAR(5) NULL,
    Latitud DECIMAL(12,6) NULL,
    Longitud DECIMAL(12,6) NULL,
    Activo BIT DEFAULT 1,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Estab_Diresa FOREIGN KEY (ID_DIRESA) REFERENCES IPRESS_Diresa(ID_DIRESA),
    CONSTRAINT FK_Estab_Red FOREIGN KEY (ID_RED) REFERENCES IPRESS_Red(ID_RED),
    CONSTRAINT FK_Estab_MicroRed FOREIGN KEY (ID_MICRORED) REFERENCES IPRESS_MicroRed(ID_MICRORED)
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_EstablecimientoCentroPoblado')
CREATE TABLE IPRESS_EstablecimientoCentroPoblado (
    ID_ESTABLECIMIENTO INT NOT NULL,
    ID_CENTRO_POBLADO INT NOT NULL,
    PRIMARY KEY (ID_ESTABLECIMIENTO, ID_CENTRO_POBLADO),
    CONSTRAINT FK_EstabCP_Estab FOREIGN KEY (ID_ESTABLECIMIENTO) REFERENCES IPRESS_Establecimiento(ID_ESTABLECIMIENTO),
    CONSTRAINT FK_EstabCP_CP FOREIGN KEY (ID_CENTRO_POBLADO) REFERENCES IPRESS_CentroPoblado(ID_CENTRO_POBLADO)
);
GO

-- Centro Poblado: columna ID_ESTABLECIMIENTO (FK a Establecimiento)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_CentroPoblado')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'ID_ESTABLECIMIENTO')
BEGIN
    ALTER TABLE IPRESS_CentroPoblado ADD ID_ESTABLECIMIENTO INT NULL;
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Establecimiento')
        ALTER TABLE IPRESS_CentroPoblado ADD CONSTRAINT FK_CP_Establecimiento FOREIGN KEY (ID_ESTABLECIMIENTO) REFERENCES IPRESS_Establecimiento(ID_ESTABLECIMIENTO);
END
GO

-- Usuario: columnas ID_RED, ID_MICRORED, ID_ESTABLECIMIENTO (después de crear Red, MicroRed, Establecimiento)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Usuario')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Usuario') AND name = 'ID_RED')
BEGIN
    ALTER TABLE IPRESS_Usuario ADD ID_RED INT NULL;
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Red')
        ALTER TABLE IPRESS_Usuario ADD CONSTRAINT FK_Usuario_Red FOREIGN KEY (ID_RED) REFERENCES IPRESS_Red(ID_RED);
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Usuario')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Usuario') AND name = 'ID_MICRORED')
BEGIN
    ALTER TABLE IPRESS_Usuario ADD ID_MICRORED INT NULL;
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_MicroRed')
        ALTER TABLE IPRESS_Usuario ADD CONSTRAINT FK_Usuario_MicroRed FOREIGN KEY (ID_MICRORED) REFERENCES IPRESS_MicroRed(ID_MICRORED);
END
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Usuario')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Usuario') AND name = 'ID_ESTABLECIMIENTO')
BEGIN
    ALTER TABLE IPRESS_Usuario ADD ID_ESTABLECIMIENTO INT NULL;
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Establecimiento')
        ALTER TABLE IPRESS_Usuario ADD CONSTRAINT FK_Usuario_Establecimiento FOREIGN KEY (ID_ESTABLECIMIENTO) REFERENCES IPRESS_Establecimiento(ID_ESTABLECIMIENTO);
END
GO

-- Migración: renombrar Contrasena a Password en IPRESS_Usuario (si existe columna antigua)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Usuario')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Usuario') AND name = 'Contrasena')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Usuario') AND name = 'Password')
BEGIN
    EXEC sp_rename 'IPRESS_Usuario.Contrasena', 'Password', 'COLUMN';
END
GO

-- =============================================
-- 6. AUDITORÍA
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Auditoria')
CREATE TABLE IPRESS_Auditoria (
    ID_AUDITORIA BIGINT PRIMARY KEY IDENTITY(1,1),
    FechaHora DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UsuarioId INT NULL,
    Tabla NVARCHAR(128) NOT NULL,
    ObjetoBD NVARCHAR(256) NULL,
    Accion CHAR(1) NOT NULL,
    IdRegistro NVARCHAR(100) NULL,
    DetalleAntes NVARCHAR(MAX) NULL,
    DetalleDespues NVARCHAR(MAX) NULL,
    IpOrigen NVARCHAR(45) NULL
);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_IPRESS_Auditoria_Fecha' AND object_id = OBJECT_ID('IPRESS_Auditoria'))
    CREATE INDEX IX_IPRESS_Auditoria_Fecha ON IPRESS_Auditoria (FechaHora DESC);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_IPRESS_Auditoria_Tabla' AND object_id = OBJECT_ID('IPRESS_Auditoria'))
    CREATE INDEX IX_IPRESS_Auditoria_Tabla ON IPRESS_Auditoria (Tabla, FechaHora DESC);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_IPRESS_Auditoria_Usuario' AND object_id = OBJECT_ID('IPRESS_Auditoria'))
    CREATE INDEX IX_IPRESS_Auditoria_Usuario ON IPRESS_Auditoria (UsuarioId, FechaHora DESC);
GO

-- =============================================
-- 7. FUNCIÓN Y PROCEDIMIENTO
-- =============================================

IF OBJECT_ID('dbo.FN_IPRESS_GetContextUserId', 'FN') IS NOT NULL
    DROP FUNCTION dbo.FN_IPRESS_GetContextUserId;
GO
CREATE FUNCTION dbo.FN_IPRESS_GetContextUserId()
RETURNS INT
AS
BEGIN
    DECLARE @ctx VARBINARY(128) = CAST(CONTEXT_INFO() AS VARBINARY(128));
    IF @ctx IS NULL OR DATALENGTH(@ctx) < 4 RETURN NULL;
    RETURN CAST(SUBSTRING(@ctx, 1, 4) AS INT);
END
GO

IF OBJECT_ID('dbo.FN_IPRESS_GetContextIp', 'FN') IS NOT NULL
    DROP FUNCTION dbo.FN_IPRESS_GetContextIp;
GO
CREATE FUNCTION dbo.FN_IPRESS_GetContextIp()
RETURNS NVARCHAR(45)
AS
BEGIN
    DECLARE @ctx VARBINARY(128) = CAST(CONTEXT_INFO() AS VARBINARY(128));
    IF @ctx IS NULL OR DATALENGTH(@ctx) < 49 RETURN NULL;
    RETURN RTRIM(CAST(SUBSTRING(@ctx, 5, 45) AS VARCHAR(45)));
END
GO

-- SP_ValidarUsuario: obtiene usuario por nombre (la app verifica Password con BCrypt)
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_ValidarUsuario')
    DROP PROCEDURE SP_ValidarUsuario;
GO
CREATE PROCEDURE SP_ValidarUsuario @Usuario VARCHAR(50), @Password VARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ID_USUARIO AS ID_USUARIO, NombreUsuario AS NombreUsuarioUSU, NombreCompleto AS ApellidosNombrePER, Password
    FROM IPRESS_Usuario
    WHERE NombreUsuario = @Usuario AND Activo = 1;
END
GO

-- SP_Usuario_ActualizarPassword: actualiza contraseña (hash) del usuario
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_Usuario_ActualizarPassword')
    DROP PROCEDURE SP_Usuario_ActualizarPassword;
GO
CREATE PROCEDURE SP_Usuario_ActualizarPassword @IdUsuario INT, @PasswordHash VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE IPRESS_Usuario SET Password = @PasswordHash WHERE ID_USUARIO = @IdUsuario;
    SELECT @@ROWCOUNT AS FilasActualizadas;
END
GO

-- SP_Auditoria_Registrar: registro explícito de auditoría (UsuarioId e IpOrigen desde parámetros)
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_Auditoria_Registrar')
    DROP PROCEDURE SP_Auditoria_Registrar;
GO
CREATE PROCEDURE SP_Auditoria_Registrar
    @UsuarioId INT = NULL,
    @IpOrigen NVARCHAR(45) = NULL,
    @Tabla NVARCHAR(128),
    @ObjetoBD NVARCHAR(256) = NULL,
    @Accion CHAR(1),
    @IdRegistro NVARCHAR(100) = NULL,
    @DetalleAntes NVARCHAR(MAX) = NULL,
    @DetalleDespues NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro, DetalleAntes, DetalleDespues)
    VALUES (@UsuarioId, @IpOrigen, @Tabla, @ObjetoBD, @Accion, @IdRegistro, @DetalleAntes, @DetalleDespues);
END
GO

-- =============================================
-- 8. TRIGGERS DE AUDITORÍA
-- =============================================

IF OBJECT_ID('TR_IPRESS_Diresa_Audit', 'TR') IS NOT NULL DROP TRIGGER TR_IPRESS_Diresa_Audit;
GO
CREATE TRIGGER TR_IPRESS_Diresa_Audit ON IPRESS_Diresa AFTER INSERT, UPDATE, DELETE
AS
BEGIN SET NOCOUNT ON;
    DECLARE @UsuarioId INT = dbo.FN_IPRESS_GetContextUserId();
    DECLARE @IpOrigen NVARCHAR(45) = dbo.FN_IPRESS_GetContextIp();
    IF EXISTS (SELECT 1 FROM inserted)
        INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro, DetalleDespues)
        SELECT @UsuarioId, @IpOrigen, 'IPRESS_Diresa', 'TR_IPRESS_Diresa_Audit', 'I', 'ID_DIRESA=' + CAST(i.ID_DIRESA AS NVARCHAR(20)),
               'Codigo='+CAST(ISNULL(i.Codigo,0) AS NVARCHAR(20))+'|Nombre='+ISNULL(i.Nombre,'')+'|Ubigeo='+ISNULL(i.Ubigeo,'') FROM inserted i;
    IF EXISTS (SELECT 1 FROM deleted) AND NOT EXISTS (SELECT 1 FROM inserted)
        INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro, DetalleAntes)
        SELECT @UsuarioId, @IpOrigen, 'IPRESS_Diresa', 'TR_IPRESS_Diresa_Audit', 'D', 'ID_DIRESA=' + CAST(d.ID_DIRESA AS NVARCHAR(20)),
               'Codigo='+CAST(ISNULL(d.Codigo,0) AS NVARCHAR(20))+'|Nombre='+ISNULL(d.Nombre,'')+'|Ubigeo='+ISNULL(d.Ubigeo,'') FROM deleted d;
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
        INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro, DetalleAntes, DetalleDespues)
        SELECT @UsuarioId, @IpOrigen, 'IPRESS_Diresa', 'TR_IPRESS_Diresa_Audit', 'U', 'ID_DIRESA=' + CAST(i.ID_DIRESA AS NVARCHAR(20)),
               'Codigo='+CAST(ISNULL(d.Codigo,0) AS NVARCHAR(20))+'|Nombre='+ISNULL(d.Nombre,'')+'|Ubigeo='+ISNULL(d.Ubigeo,''),
               'Codigo='+CAST(ISNULL(i.Codigo,0) AS NVARCHAR(20))+'|Nombre='+ISNULL(i.Nombre,'')+'|Ubigeo='+ISNULL(i.Ubigeo,'')
        FROM inserted i INNER JOIN deleted d ON i.ID_DIRESA = d.ID_DIRESA;
END
GO

IF OBJECT_ID('TR_IPRESS_Red_Audit', 'TR') IS NOT NULL DROP TRIGGER TR_IPRESS_Red_Audit;
GO
CREATE TRIGGER TR_IPRESS_Red_Audit ON IPRESS_Red AFTER INSERT, UPDATE, DELETE
AS
BEGIN SET NOCOUNT ON;
    DECLARE @UsuarioId INT = dbo.FN_IPRESS_GetContextUserId();
    DECLARE @IpOrigen NVARCHAR(45) = dbo.FN_IPRESS_GetContextIp();
    IF EXISTS (SELECT 1 FROM inserted)
        INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro)
        SELECT @UsuarioId, @IpOrigen, 'IPRESS_Red', 'TR_IPRESS_Red_Audit', 'I', 'ID_RED=' + CAST(i.ID_RED AS NVARCHAR(20)) FROM inserted i;
    IF EXISTS (SELECT 1 FROM deleted) AND NOT EXISTS (SELECT 1 FROM inserted)
        INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro, DetalleAntes)
        SELECT @UsuarioId, @IpOrigen, 'IPRESS_Red', 'TR_IPRESS_Red_Audit', 'D', 'ID_RED=' + CAST(d.ID_RED AS NVARCHAR(20)),
               'Codigo='+CAST(ISNULL(d.Codigo,0) AS NVARCHAR(20))+'|Nombre='+ISNULL(d.Nombre,'')+'|Ubigeo='+ISNULL(d.Ubigeo,'') FROM deleted d;
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
        INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro, DetalleAntes, DetalleDespues)
        SELECT @UsuarioId, @IpOrigen, 'IPRESS_Red', 'TR_IPRESS_Red_Audit', 'U', 'ID_RED=' + CAST(i.ID_RED AS NVARCHAR(20)),
               'Codigo='+CAST(ISNULL(d.Codigo,0) AS NVARCHAR(20))+'|Nombre='+ISNULL(d.Nombre,'')+'|Ubigeo='+ISNULL(d.Ubigeo,''), 'Codigo='+CAST(ISNULL(i.Codigo,0) AS NVARCHAR(20))+'|Nombre='+ISNULL(i.Nombre,'')+'|Ubigeo='+ISNULL(i.Ubigeo,'')
        FROM inserted i INNER JOIN deleted d ON i.ID_RED = d.ID_RED;
END
GO

IF OBJECT_ID('TR_IPRESS_MicroRed_Audit', 'TR') IS NOT NULL DROP TRIGGER TR_IPRESS_MicroRed_Audit;
GO
CREATE TRIGGER TR_IPRESS_MicroRed_Audit ON IPRESS_MicroRed AFTER INSERT, UPDATE, DELETE
AS
BEGIN SET NOCOUNT ON;
    DECLARE @UsuarioId INT = dbo.FN_IPRESS_GetContextUserId();
    DECLARE @IpOrigen NVARCHAR(45) = dbo.FN_IPRESS_GetContextIp();
    IF EXISTS (SELECT 1 FROM inserted)
        INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro)
        SELECT @UsuarioId, @IpOrigen, 'IPRESS_MicroRed', 'TR_IPRESS_MicroRed_Audit', 'I', 'ID_MICRORED=' + CAST(i.ID_MICRORED AS NVARCHAR(20)) FROM inserted i;
    IF EXISTS (SELECT 1 FROM deleted) AND NOT EXISTS (SELECT 1 FROM inserted)
        INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro, DetalleAntes)
        SELECT @UsuarioId, @IpOrigen, 'IPRESS_MicroRed', 'TR_IPRESS_MicroRed_Audit', 'D', 'ID_MICRORED=' + CAST(d.ID_MICRORED AS NVARCHAR(20)),
               'Codigo='+CAST(ISNULL(d.Codigo,0) AS NVARCHAR(20))+'|Nombre='+ISNULL(d.Nombre,'')+'|Ubigeo='+ISNULL(d.Ubigeo,'') FROM deleted d;
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
        INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro, DetalleAntes, DetalleDespues)
        SELECT @UsuarioId, @IpOrigen, 'IPRESS_MicroRed', 'TR_IPRESS_MicroRed_Audit', 'U', 'ID_MICRORED=' + CAST(i.ID_MICRORED AS NVARCHAR(20)),
               'Codigo='+CAST(ISNULL(d.Codigo,0) AS NVARCHAR(20))+'|Nombre='+ISNULL(d.Nombre,'')+'|Ubigeo='+ISNULL(d.Ubigeo,''), 'Codigo='+CAST(ISNULL(i.Codigo,0) AS NVARCHAR(20))+'|Nombre='+ISNULL(i.Nombre,'')+'|Ubigeo='+ISNULL(i.Ubigeo,'')
        FROM inserted i INNER JOIN deleted d ON i.ID_MICRORED = d.ID_MICRORED;
END
GO

IF OBJECT_ID('TR_IPRESS_Establecimiento_Audit', 'TR') IS NOT NULL DROP TRIGGER TR_IPRESS_Establecimiento_Audit;
GO
CREATE TRIGGER TR_IPRESS_Establecimiento_Audit ON IPRESS_Establecimiento AFTER INSERT, UPDATE, DELETE
AS
BEGIN SET NOCOUNT ON;
    DECLARE @UsuarioId INT = dbo.FN_IPRESS_GetContextUserId();
    DECLARE @IpOrigen NVARCHAR(45) = dbo.FN_IPRESS_GetContextIp();
    IF EXISTS (SELECT 1 FROM inserted)
        INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro)
        SELECT @UsuarioId, @IpOrigen, 'IPRESS_Establecimiento', 'TR_IPRESS_Establecimiento_Audit', 'I', 'ID_ESTABLECIMIENTO=' + CAST(i.ID_ESTABLECIMIENTO AS NVARCHAR(20)) FROM inserted i;
    IF EXISTS (SELECT 1 FROM deleted) AND NOT EXISTS (SELECT 1 FROM inserted)
        INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro, DetalleAntes)
        SELECT @UsuarioId, @IpOrigen, 'IPRESS_Establecimiento', 'TR_IPRESS_Establecimiento_Audit', 'D', 'ID_ESTABLECIMIENTO=' + CAST(d.ID_ESTABLECIMIENTO AS NVARCHAR(20)),
               'Codigo='+ISNULL(d.Codigo,'')+'|Nombre='+ISNULL(d.Nombre,'') FROM deleted d;
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
        INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro, DetalleAntes, DetalleDespues)
        SELECT @UsuarioId, @IpOrigen, 'IPRESS_Establecimiento', 'TR_IPRESS_Establecimiento_Audit', 'U', 'ID_ESTABLECIMIENTO=' + CAST(i.ID_ESTABLECIMIENTO AS NVARCHAR(20)),
               'Codigo='+ISNULL(d.Codigo,'')+'|Nombre='+ISNULL(d.Nombre,''), 'Codigo='+ISNULL(i.Codigo,'')+'|Nombre='+ISNULL(i.Nombre,'')
        FROM inserted i INNER JOIN deleted d ON i.ID_ESTABLECIMIENTO = d.ID_ESTABLECIMIENTO;
END
GO

IF OBJECT_ID('TR_IPRESS_Usuario_Audit', 'TR') IS NOT NULL DROP TRIGGER TR_IPRESS_Usuario_Audit;
GO
CREATE TRIGGER TR_IPRESS_Usuario_Audit ON IPRESS_Usuario AFTER INSERT, UPDATE, DELETE
AS
BEGIN SET NOCOUNT ON;
    DECLARE @UsuarioId INT = dbo.FN_IPRESS_GetContextUserId();
    DECLARE @IpOrigen NVARCHAR(45) = dbo.FN_IPRESS_GetContextIp();
    IF EXISTS (SELECT 1 FROM inserted)
        INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro, DetalleDespues)
        SELECT @UsuarioId, @IpOrigen, 'IPRESS_Usuario', 'TR_IPRESS_Usuario_Audit', 'I', 'ID_USUARIO=' + CAST(i.ID_USUARIO AS NVARCHAR(20)),
               'NombreUsuario='+ISNULL(i.NombreUsuario,'')+'|NombreCompleto='+ISNULL(i.NombreCompleto,'')+'|Activo='+CAST(ISNULL(i.Activo,0) AS NVARCHAR(1)) FROM inserted i;
    IF EXISTS (SELECT 1 FROM deleted) AND NOT EXISTS (SELECT 1 FROM inserted)
        INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro, DetalleAntes)
        SELECT @UsuarioId, @IpOrigen, 'IPRESS_Usuario', 'TR_IPRESS_Usuario_Audit', 'D', 'ID_USUARIO=' + CAST(d.ID_USUARIO AS NVARCHAR(20)),
               'NombreUsuario='+ISNULL(d.NombreUsuario,'')+'|NombreCompleto='+ISNULL(d.NombreCompleto,'')+'|Activo='+CAST(ISNULL(d.Activo,0) AS NVARCHAR(1)) FROM deleted d;
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
        INSERT INTO IPRESS_Auditoria (UsuarioId, IpOrigen, Tabla, ObjetoBD, Accion, IdRegistro, DetalleAntes, DetalleDespues)
        SELECT @UsuarioId, @IpOrigen, 'IPRESS_Usuario', 'TR_IPRESS_Usuario_Audit', 'U', 'ID_USUARIO=' + CAST(i.ID_USUARIO AS NVARCHAR(20)),
               'NombreUsuario='+ISNULL(d.NombreUsuario,'')+'|NombreCompleto='+ISNULL(d.NombreCompleto,'')+'|Activo='+CAST(ISNULL(d.Activo,0) AS NVARCHAR(1)),
               'NombreUsuario='+ISNULL(i.NombreUsuario,'')+'|NombreCompleto='+ISNULL(i.NombreCompleto,'')+'|Activo='+CAST(ISNULL(i.Activo,0) AS NVARCHAR(1))
        FROM inserted i INNER JOIN deleted d ON i.ID_USUARIO = d.ID_USUARIO;
END
GO

-- =============================================
-- 9. REAJUSTAR IDENTITY DE AUDITORÍA (evita duplicate key en importaciones masivas)
-- =============================================
-- Si el IDENTITY de IPRESS_Auditoria queda desincronizado (p. ej. tras borrados o restauración),
-- el próximo INSERT puede intentar usar un ID ya existente. Reseed al máximo actual.
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Auditoria')
BEGIN
    DECLARE @maxAudit BIGINT = (SELECT ISNULL(MAX(ID_AUDITORIA), 0) FROM IPRESS_Auditoria);
    DBCC CHECKIDENT ('IPRESS_Auditoria', RESEED, @maxAudit);
END
GO

-- =============================================
-- FIN DDL
-- =============================================
PRINT 'IPRESS_DDL.sql: Estructura de base de datos aplicada. Ejecute IPRESS_Inserts.sql para datos iniciales.';
