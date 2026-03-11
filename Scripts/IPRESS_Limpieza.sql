-- =============================================
-- IPRESS - SCRIPT ÚNICO DE LIMPIEZA DE BASE DE DATOS
-- =============================================
-- Incluye: desvincular FKs (UPDATE a NULL), DELETE/TRUNCATE de tablas
-- en orden según dependencias. No elimina la estructura (tablas/SP/triggers);
-- solo vacía datos. Para reset completo ejecute antes IPRESS_DDL.sql en BD nueva.
--
-- Ejecutar sobre la BD IPRESS (USE IPRESS).
-- =============================================

SET NOCOUNT ON;
GO

USE IPRESS;
GO

-- =============================================
-- 1. DESVINCULAR REFERENCIAS (poner NULL en FKs)
-- =============================================

-- Usuario: desvincular Diresa, Red, MicroRed, Establecimiento
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Usuario')
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Usuario') AND name = 'ID_DIRESA')
        UPDATE IPRESS_Usuario SET ID_DIRESA = NULL WHERE ID_DIRESA IS NOT NULL;
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Usuario') AND name = 'ID_RED')
        UPDATE IPRESS_Usuario SET ID_RED = NULL WHERE ID_RED IS NOT NULL;
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Usuario') AND name = 'ID_MICRORED')
        UPDATE IPRESS_Usuario SET ID_MICRORED = NULL WHERE ID_MICRORED IS NOT NULL;
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_Usuario') AND name = 'ID_ESTABLECIMIENTO')
        UPDATE IPRESS_Usuario SET ID_ESTABLECIMIENTO = NULL WHERE ID_ESTABLECIMIENTO IS NOT NULL;
END

-- Establecimiento: desvincular Diresa, Red, MicroRed
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Establecimiento')
    UPDATE IPRESS_Establecimiento SET ID_DIRESA = NULL, ID_RED = NULL, ID_MICRORED = NULL
    WHERE ID_DIRESA IS NOT NULL OR ID_RED IS NOT NULL OR ID_MICRORED IS NOT NULL;

-- Centro Poblado: desvincular Establecimiento (si existe la columna)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_CentroPoblado')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IPRESS_CentroPoblado') AND name = 'ID_ESTABLECIMIENTO')
    UPDATE IPRESS_CentroPoblado SET ID_ESTABLECIMIENTO = NULL WHERE ID_ESTABLECIMIENTO IS NOT NULL;

GO

-- =============================================
-- 2. ELIMINAR DATOS EN ORDEN (respetando FKs)
-- =============================================

-- Tablas de permisos y asignaciones (referencian Usuario, Rol, Modulo, Menu, etc.)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_RolSubMenu')    DELETE FROM IPRESS_RolSubMenu;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_RolBoton')      DELETE FROM IPRESS_RolBoton;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_RolModulo')      DELETE FROM IPRESS_RolModulo;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_UsuarioRol')    DELETE FROM IPRESS_UsuarioRol;

-- Auditoría (log)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Auditoria')     DELETE FROM IPRESS_Auditoria;

-- Centros Poblados: tablas hijas (CASCADE las borraría al borrar padre; por si se ejecuta solo limpieza)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_CentroPobladoAccesibilidad')   DELETE FROM IPRESS_CentroPobladoAccesibilidad;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_CentroPobladoCentroEducativo') DELETE FROM IPRESS_CentroPobladoCentroEducativo;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_CentroPobladoAutoridad')        DELETE FROM IPRESS_CentroPobladoAutoridad;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_EstablecimientoCentroPoblado')  DELETE FROM IPRESS_EstablecimientoCentroPoblado;

-- Maestros y entidades principales
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_CentroPoblado')     DELETE FROM IPRESS_CentroPoblado;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Establecimiento')   DELETE FROM IPRESS_Establecimiento;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_MicroRed')         DELETE FROM IPRESS_MicroRed;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Red')               DELETE FROM IPRESS_Red;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Diresa')             DELETE FROM IPRESS_Diresa;

-- Seguridad
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Usuario')       DELETE FROM IPRESS_Usuario;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_SubMenu')       DELETE FROM IPRESS_SubMenu;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Menu')          DELETE FROM IPRESS_Menu;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Boton')         DELETE FROM IPRESS_Boton;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Rol')           DELETE FROM IPRESS_Rol;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Modulo')        DELETE FROM IPRESS_Modulo;

-- Geografía
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Distrito')      DELETE FROM IPRESS_Distrito;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Provincia')     DELETE FROM IPRESS_Provincia;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Departamento') DELETE FROM IPRESS_Departamento;

GO

-- =============================================
-- 3. RESTAURAR IDENTITY (IDs desde 1 tras repoblar)
-- =============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Modulo')              DBCC CHECKIDENT ('IPRESS_Modulo', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Boton')                DBCC CHECKIDENT ('IPRESS_Boton', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Rol')                  DBCC CHECKIDENT ('IPRESS_Rol', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Usuario')              DBCC CHECKIDENT ('IPRESS_Usuario', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Menu')                 DBCC CHECKIDENT ('IPRESS_Menu', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_SubMenu')              DBCC CHECKIDENT ('IPRESS_SubMenu', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Departamento')         DBCC CHECKIDENT ('IPRESS_Departamento', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Provincia')            DBCC CHECKIDENT ('IPRESS_Provincia', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Distrito')            DBCC CHECKIDENT ('IPRESS_Distrito', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Diresa')               DBCC CHECKIDENT ('IPRESS_Diresa', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Red')                  DBCC CHECKIDENT ('IPRESS_Red', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_MicroRed')             DBCC CHECKIDENT ('IPRESS_MicroRed', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_CentroPoblado')        DBCC CHECKIDENT ('IPRESS_CentroPoblado', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_CentroPobladoAccesibilidad')   DBCC CHECKIDENT ('IPRESS_CentroPobladoAccesibilidad', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_CentroPobladoCentroEducativo') DBCC CHECKIDENT ('IPRESS_CentroPobladoCentroEducativo', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_CentroPobladoAutoridad')        DBCC CHECKIDENT ('IPRESS_CentroPobladoAutoridad', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Establecimiento')      DBCC CHECKIDENT ('IPRESS_Establecimiento', RESEED, 0);
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IPRESS_Auditoria')             DBCC CHECKIDENT ('IPRESS_Auditoria', RESEED, 0);
GO

-- =============================================
-- FIN LIMPIEZA
-- =============================================
PRINT 'IPRESS_Limpieza.sql: Tablas vaciadas e IDs restaurados. Ejecute IPRESS_Inserts.sql para repoblar datos iniciales.';
