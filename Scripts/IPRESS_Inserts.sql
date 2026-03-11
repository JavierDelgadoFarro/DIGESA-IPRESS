-- =============================================
-- IPRESS - SCRIPT ÚNICO DE INSERTS (DATOS INICIALES)
-- =============================================
-- Incluye todos los INSERT de tablas del proyecto: geografía, módulos,
-- botones, roles, usuarios, menús, submenús y permisos.
-- Ejecutar después de IPRESS_DDL.sql (USE IPRESS).
--
-- Opcional: para cargar el ubigeo completo de Perú, ejecute además
-- Insert_Ubigeo_Peru_Completo.sql
-- =============================================

SET NOCOUNT ON;
GO

USE IPRESS;
GO

-- =============================================
-- 1. GEOGRAFÍA PERÚ (muestra Lima; opcional: Insert_Ubigeo_Peru_Completo.sql)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM IPRESS_Departamento)
BEGIN
    INSERT INTO IPRESS_Departamento (Codigo, Nombre) VALUES ('15', 'LIMA');
    INSERT INTO IPRESS_Provincia (Codigo, Nombre, CodigoDepartamento) VALUES ('1501', 'LIMA', '15');
    INSERT INTO IPRESS_Distrito (Ubigeo, Nombre, CodigoProvincia) VALUES ('150101', 'LIMA', '1501');
    INSERT INTO IPRESS_Distrito (Ubigeo, Nombre, CodigoProvincia) VALUES ('150102', 'ANCON', '1501');
    INSERT INTO IPRESS_Distrito (Ubigeo, Nombre, CodigoProvincia) VALUES ('150103', 'ATE', '1501');
END
GO

-- =============================================
-- 2. MÓDULOS
-- =============================================
IF NOT EXISTS (SELECT 1 FROM IPRESS_Modulo)
BEGIN
    INSERT INTO IPRESS_Modulo (Codigo, Nombre, Ruta, Orden) VALUES
    ('DIRESAS', 'Mantenimiento de Diresas', '/diresas', 1),
    ('RED', 'Mantenimiento de Red', '/red', 2),
    ('MICRORED', 'Mantenimiento de MicroRed', '/microred', 3),
    ('ESTABLECIMIENTOS', 'Mantenimiento de Establecimiento de Salud', '/establecimientos', 4),
    ('CENTROSPOBLADOS', 'Centro Poblado', '/centros-poblados', 5),
    ('USUARIOS', 'Gestión de Usuarios', '/usuarios', 6),
    ('ROLES', 'Gestión de Roles', '/roles', 7);
END
GO

IF NOT EXISTS (SELECT 1 FROM IPRESS_Modulo WHERE Codigo = 'ROLES')
    INSERT INTO IPRESS_Modulo (Codigo, Nombre, Ruta, Orden) VALUES ('ROLES', 'Gestión de Roles', '/roles', 7);
IF NOT EXISTS (SELECT 1 FROM IPRESS_Modulo WHERE Codigo = 'CENTROSPOBLADOS')
    INSERT INTO IPRESS_Modulo (Codigo, Nombre, Ruta, Orden) VALUES ('CENTROSPOBLADOS', 'Centro Poblado', '/centros-poblados', 5);
GO

-- =============================================
-- 3. BOTONES POR MÓDULO
-- =============================================
IF NOT EXISTS (SELECT 1 FROM IPRESS_Boton)
BEGIN
    INSERT INTO IPRESS_Boton (ID_MODULO, Codigo, Nombre)
    SELECT ID_MODULO, 'CREAR', 'Crear' FROM IPRESS_Modulo WHERE Codigo = 'DIRESAS'
    UNION ALL SELECT ID_MODULO, 'EDITAR', 'Editar' FROM IPRESS_Modulo WHERE Codigo = 'DIRESAS'
    UNION ALL SELECT ID_MODULO, 'ELIMINAR', 'Eliminar' FROM IPRESS_Modulo WHERE Codigo = 'DIRESAS'
    UNION ALL SELECT ID_MODULO, 'IMPORTAR', 'Importar Excel' FROM IPRESS_Modulo WHERE Codigo = 'DIRESAS'
    UNION ALL SELECT ID_MODULO, 'EXPORTAR', 'Exportar' FROM IPRESS_Modulo WHERE Codigo = 'DIRESAS'
    UNION ALL SELECT ID_MODULO, 'CREAR', 'Crear' FROM IPRESS_Modulo WHERE Codigo = 'RED'
    UNION ALL SELECT ID_MODULO, 'EDITAR', 'Editar' FROM IPRESS_Modulo WHERE Codigo = 'RED'
    UNION ALL SELECT ID_MODULO, 'ELIMINAR', 'Eliminar' FROM IPRESS_Modulo WHERE Codigo = 'RED'
    UNION ALL SELECT ID_MODULO, 'IMPORTAR', 'Importar Excel' FROM IPRESS_Modulo WHERE Codigo = 'RED'
    UNION ALL SELECT ID_MODULO, 'EXPORTAR', 'Exportar' FROM IPRESS_Modulo WHERE Codigo = 'RED'
    UNION ALL SELECT ID_MODULO, 'CREAR', 'Crear' FROM IPRESS_Modulo WHERE Codigo = 'MICRORED'
    UNION ALL SELECT ID_MODULO, 'EDITAR', 'Editar' FROM IPRESS_Modulo WHERE Codigo = 'MICRORED'
    UNION ALL SELECT ID_MODULO, 'ELIMINAR', 'Eliminar' FROM IPRESS_Modulo WHERE Codigo = 'MICRORED'
    UNION ALL SELECT ID_MODULO, 'IMPORTAR', 'Importar Excel' FROM IPRESS_Modulo WHERE Codigo = 'MICRORED'
    UNION ALL SELECT ID_MODULO, 'EXPORTAR', 'Exportar' FROM IPRESS_Modulo WHERE Codigo = 'MICRORED'
    UNION ALL SELECT ID_MODULO, 'CREAR', 'Crear' FROM IPRESS_Modulo WHERE Codigo = 'ESTABLECIMIENTOS'
    UNION ALL SELECT ID_MODULO, 'EDITAR', 'Editar' FROM IPRESS_Modulo WHERE Codigo = 'ESTABLECIMIENTOS'
    UNION ALL SELECT ID_MODULO, 'ELIMINAR', 'Eliminar' FROM IPRESS_Modulo WHERE Codigo = 'ESTABLECIMIENTOS'
    UNION ALL SELECT ID_MODULO, 'EXPORTAR', 'Exportar' FROM IPRESS_Modulo WHERE Codigo = 'ESTABLECIMIENTOS'
    UNION ALL SELECT ID_MODULO, 'CREAR', 'Crear' FROM IPRESS_Modulo WHERE Codigo = 'CENTROSPOBLADOS'
    UNION ALL SELECT ID_MODULO, 'EDITAR', 'Editar' FROM IPRESS_Modulo WHERE Codigo = 'CENTROSPOBLADOS'
    UNION ALL SELECT ID_MODULO, 'ELIMINAR', 'Eliminar' FROM IPRESS_Modulo WHERE Codigo = 'CENTROSPOBLADOS'
    UNION ALL SELECT ID_MODULO, 'EXPORTAR', 'Exportar' FROM IPRESS_Modulo WHERE Codigo = 'CENTROSPOBLADOS'
    UNION ALL SELECT ID_MODULO, 'CREAR', 'Crear' FROM IPRESS_Modulo WHERE Codigo = 'USUARIOS'
    UNION ALL SELECT ID_MODULO, 'EDITAR', 'Editar' FROM IPRESS_Modulo WHERE Codigo = 'USUARIOS'
    UNION ALL SELECT ID_MODULO, 'ELIMINAR', 'Eliminar' FROM IPRESS_Modulo WHERE Codigo = 'USUARIOS'
    UNION ALL SELECT ID_MODULO, 'CREAR', 'Crear' FROM IPRESS_Modulo WHERE Codigo = 'ROLES'
    UNION ALL SELECT ID_MODULO, 'EDITAR', 'Editar' FROM IPRESS_Modulo WHERE Codigo = 'ROLES'
    UNION ALL SELECT ID_MODULO, 'ELIMINAR', 'Eliminar' FROM IPRESS_Modulo WHERE Codigo = 'ROLES';
END
GO

-- Botones ROLES y CENTROSPOBLADOS (por si los módulos se agregaron después)
IF NOT EXISTS (SELECT 1 FROM IPRESS_Boton b INNER JOIN IPRESS_Modulo m ON b.ID_MODULO = m.ID_MODULO WHERE m.Codigo = 'ROLES')
    INSERT INTO IPRESS_Boton (ID_MODULO, Codigo, Nombre)
    SELECT ID_MODULO, 'CREAR', 'Crear' FROM IPRESS_Modulo WHERE Codigo = 'ROLES'
    UNION ALL SELECT ID_MODULO, 'EDITAR', 'Editar' FROM IPRESS_Modulo WHERE Codigo = 'ROLES'
    UNION ALL SELECT ID_MODULO, 'ELIMINAR', 'Eliminar' FROM IPRESS_Modulo WHERE Codigo = 'ROLES';
IF NOT EXISTS (SELECT 1 FROM IPRESS_Boton b INNER JOIN IPRESS_Modulo m ON b.ID_MODULO = m.ID_MODULO WHERE m.Codigo = 'CENTROSPOBLADOS')
    INSERT INTO IPRESS_Boton (ID_MODULO, Codigo, Nombre)
    SELECT ID_MODULO, 'CREAR', 'Crear' FROM IPRESS_Modulo WHERE Codigo = 'CENTROSPOBLADOS'
    UNION ALL SELECT ID_MODULO, 'EDITAR', 'Editar' FROM IPRESS_Modulo WHERE Codigo = 'CENTROSPOBLADOS'
    UNION ALL SELECT ID_MODULO, 'ELIMINAR', 'Eliminar' FROM IPRESS_Modulo WHERE Codigo = 'CENTROSPOBLADOS'
    UNION ALL SELECT ID_MODULO, 'EXPORTAR', 'Exportar' FROM IPRESS_Modulo WHERE Codigo = 'CENTROSPOBLADOS';
GO

-- =============================================
-- 4. ROL ADMIN Y PERMISOS
-- =============================================
IF NOT EXISTS (SELECT 1 FROM IPRESS_Rol WHERE Codigo = 'ADMIN')
BEGIN
    INSERT INTO IPRESS_Rol (Codigo, Nombre) VALUES ('ADMIN', 'Administrador');
    INSERT INTO IPRESS_RolModulo (ID_ROL, ID_MODULO)
    SELECT (SELECT ID_ROL FROM IPRESS_Rol WHERE Codigo = 'ADMIN'), ID_MODULO FROM IPRESS_Modulo;
    INSERT INTO IPRESS_RolBoton (ID_ROL, ID_BOTON)
    SELECT (SELECT ID_ROL FROM IPRESS_Rol WHERE Codigo = 'ADMIN'), ID_BOTON FROM IPRESS_Boton;
END
GO

-- =============================================
-- 5. USUARIO admin
-- Contraseña inicial "admin": la aplicación la hashea con BCrypt en el primer login
-- y la verifica con BCrypt.Verify (no hay desencriptación; solo verificación).
-- =============================================
IF NOT EXISTS (SELECT 1 FROM IPRESS_Usuario WHERE NombreUsuario = 'admin')
BEGIN
    INSERT INTO IPRESS_Usuario (NombreUsuario, Password, NombreCompleto, Activo)
    VALUES ('admin', 'admin', 'Administrador del Sistema', 1);
    INSERT INTO IPRESS_UsuarioRol (ID_USUARIO, ID_ROL)
    SELECT ID_USUARIO, (SELECT ID_ROL FROM IPRESS_Rol WHERE Codigo = 'ADMIN')
    FROM IPRESS_Usuario WHERE NombreUsuario = 'admin';
END
GO

-- =============================================
-- 6. MENÚS Y SUBMENÚS
-- =============================================
IF NOT EXISTS (SELECT 1 FROM IPRESS_Menu)
BEGIN
    INSERT INTO IPRESS_Menu (ID_MODULO, Codigo, Nombre, Orden)
    SELECT TOP 1 ID_MODULO, 'MAESTROS', 'Maestros', 1 FROM IPRESS_Modulo ORDER BY Orden;

    INSERT INTO IPRESS_SubMenu (ID_MENU, Codigo, Nombre, Ruta, Icono, Orden)
    SELECT (SELECT TOP 1 ID_MENU FROM IPRESS_Menu), Codigo, Nombre, ISNULL(NULLIF(Ruta,''), '/' + LOWER(REPLACE(Codigo,'_','-'))), NULL, Orden
    FROM IPRESS_Modulo ORDER BY Orden;

    IF NOT EXISTS (SELECT 1 FROM IPRESS_SubMenu WHERE Ruta = '/principal')
        INSERT INTO IPRESS_SubMenu (ID_MENU, Codigo, Nombre, Ruta, Icono, Orden)
        VALUES ((SELECT TOP 1 ID_MENU FROM IPRESS_Menu), 'INICIO', 'Inicio', '/principal', 'home', 0);
END
GO

IF NOT EXISTS (SELECT 1 FROM IPRESS_SubMenu WHERE Ruta = '/roles')
    INSERT INTO IPRESS_SubMenu (ID_MENU, Codigo, Nombre, Ruta, Icono, Orden)
    SELECT (SELECT TOP 1 ID_MENU FROM IPRESS_Menu), 'ROLES', m.Nombre, m.Ruta, NULL, m.Orden
    FROM IPRESS_Modulo m WHERE m.Codigo = 'ROLES';
IF NOT EXISTS (SELECT 1 FROM IPRESS_SubMenu WHERE Ruta = '/centros-poblados')
    INSERT INTO IPRESS_SubMenu (ID_MENU, Codigo, Nombre, Ruta, Icono, Orden)
    SELECT (SELECT TOP 1 ID_MENU FROM IPRESS_Menu), 'CENTROSPOBLADOS', m.Nombre, m.Ruta, 'place', m.Orden
    FROM IPRESS_Modulo m WHERE m.Codigo = 'CENTROSPOBLADOS';
GO

-- Asignar todos los submenús al rol ADMIN
IF EXISTS (SELECT 1 FROM IPRESS_Rol WHERE Codigo = 'ADMIN')
    INSERT INTO IPRESS_RolSubMenu (ID_ROL, ID_SUBMENU)
    SELECT r.ID_ROL, s.ID_SUBMENU
    FROM IPRESS_Rol r CROSS JOIN IPRESS_SubMenu s
    WHERE r.Codigo = 'ADMIN'
    AND NOT EXISTS (SELECT 1 FROM IPRESS_RolSubMenu rs WHERE rs.ID_ROL = r.ID_ROL AND rs.ID_SUBMENU = s.ID_SUBMENU);
GO

-- =============================================
-- 7. USUARIOS DE PRUEBA (prueba, demo)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM IPRESS_Usuario WHERE NombreUsuario = 'prueba')
BEGIN
    INSERT INTO IPRESS_Usuario (NombreUsuario, Password, NombreCompleto, Email, Activo)
    VALUES ('prueba', 'prueba', 'Usuario de Prueba', 'prueba@ipress.gob.pe', 1);
    IF NOT EXISTS (SELECT 1 FROM IPRESS_Rol WHERE Codigo = 'OPERADOR')
        INSERT INTO IPRESS_Rol (Codigo, Nombre) VALUES ('OPERADOR', 'Operador');
    DECLARE @IdRolOp INT = (SELECT ID_ROL FROM IPRESS_Rol WHERE Codigo = 'OPERADOR');
    DECLARE @IdUsuPrueba INT = (SELECT ID_USUARIO FROM IPRESS_Usuario WHERE NombreUsuario = 'prueba');
    INSERT INTO IPRESS_UsuarioRol (ID_USUARIO, ID_ROL) VALUES (@IdUsuPrueba, @IdRolOp);
    INSERT INTO IPRESS_RolSubMenu (ID_ROL, ID_SUBMENU) SELECT @IdRolOp, ID_SUBMENU FROM IPRESS_SubMenu;
    INSERT INTO IPRESS_RolBoton (ID_ROL, ID_BOTON)
    SELECT @IdRolOp, ID_BOTON FROM IPRESS_Boton WHERE Codigo = 'EXPORTAR';
END
GO

IF NOT EXISTS (SELECT 1 FROM IPRESS_Usuario WHERE NombreUsuario = 'demo')
BEGIN
    INSERT INTO IPRESS_Usuario (NombreUsuario, Password, NombreCompleto, Email, Activo)
    VALUES ('demo', 'demo', 'Usuario Demo', 'demo@ipress.gob.pe', 1);
    IF NOT EXISTS (SELECT 1 FROM IPRESS_Rol WHERE Codigo = 'MAESTROS')
        INSERT INTO IPRESS_Rol (Codigo, Nombre) VALUES ('MAESTROS', 'Responsable de Maestros');
    DECLARE @IdRolMae INT = (SELECT ID_ROL FROM IPRESS_Rol WHERE Codigo = 'MAESTROS');
    DECLARE @IdUsuDemo INT = (SELECT ID_USUARIO FROM IPRESS_Usuario WHERE NombreUsuario = 'demo');
    INSERT INTO IPRESS_UsuarioRol (ID_USUARIO, ID_ROL) VALUES (@IdUsuDemo, @IdRolMae);
    INSERT INTO IPRESS_RolSubMenu (ID_ROL, ID_SUBMENU)
    SELECT @IdRolMae, ID_SUBMENU FROM IPRESS_SubMenu WHERE Codigo IN ('INICIO', 'DIRESAS', 'RED', 'MICRORED', 'ESTABLECIMIENTOS', 'CENTROSPOBLADOS');
    INSERT INTO IPRESS_RolBoton (ID_ROL, ID_BOTON)
    SELECT @IdRolMae, b.ID_BOTON FROM IPRESS_Boton b
    INNER JOIN IPRESS_Modulo m ON b.ID_MODULO = m.ID_MODULO
    WHERE m.Codigo IN ('DIRESAS', 'RED', 'MICRORED', 'ESTABLECIMIENTOS', 'CENTROSPOBLADOS');
END
GO

-- =============================================
-- FIN INSERTS
-- =============================================
PRINT 'IPRESS_Inserts.sql: Datos iniciales aplicados.';
PRINT '  Usuarios: admin/admin, prueba/prueba, demo/demo';
PRINT '  Opcional: ejecute Insert_Ubigeo_Peru_Completo.sql para ubigeo completo.';
