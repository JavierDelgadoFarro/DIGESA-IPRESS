-- =============================================
-- Script: Creación de módulo de Atenciones
-- Fecha: 2026-02-21
-- Descripción: Tablas normalizadas para gestión de atenciones
-- =============================================

-- 1. Tabla de Estados de Atención
CREATE TABLE UTD_ESTADO_ATENCION (
    ID_ESTADO_ATENCION INT PRIMARY KEY IDENTITY(1,1),
    NOMBRE_ESTADO VARCHAR(50) NOT NULL,
    ORDEN INT NOT NULL,
    DESCRIPCION VARCHAR(200),
    ESTADO BIT NOT NULL DEFAULT 1,
    FECHA_CREACION DATETIME NOT NULL DEFAULT GETDATE()
);

-- 2. Tabla de Tipos de Trámite
CREATE TABLE UTD_TIPO_TRAMITE (
    ID_TIPO_TRAMITE INT PRIMARY KEY IDENTITY(1,1),
    NOMBRE_TRAMITE VARCHAR(100) NOT NULL,
    DESCRIPCION VARCHAR(500),
    ESTADO BIT NOT NULL DEFAULT 1,
    FECHA_CREACION DATETIME NOT NULL DEFAULT GETDATE()
);

-- 3. Tabla de Tipos Preferencial
CREATE TABLE UTD_TIPO_PREFERENCIAL (
    ID_TIPO_PREFERENCIAL INT PRIMARY KEY IDENTITY(1,1),
    NOMBRE_TIPO_PREFERENCIAL VARCHAR(100) NOT NULL,
    DESCRIPCION VARCHAR(200),
    ESTADO BIT NOT NULL DEFAULT 1,
    FECHA_CREACION DATETIME NOT NULL DEFAULT GETDATE()
);

-- 4. Tabla Principal de Atenciones
CREATE TABLE UTD_ATENCIONES (
    ID_ATENCION INT PRIMARY KEY IDENTITY(1,1),
    TIPO_DOCUMENTO VARCHAR(10) NOT NULL, -- 'DNI' o 'CE'
    NUMERO_DOCUMENTO VARCHAR(20) NOT NULL,
    NOMBRES VARCHAR(150) NOT NULL,
    APELLIDOS VARCHAR(150) NOT NULL,
    ID_TIPO_TRAMITE INT NOT NULL,
    OBSERVACION VARCHAR(500) NULL,
    ES_PREFERENCIAL BIT NOT NULL DEFAULT 0,
    ID_TIPO_PREFERENCIAL INT NULL,
    ID_ESTADO_ATENCION INT NOT NULL,
    FECHA_REGISTRO DATETIME NOT NULL DEFAULT GETDATE(),
    FECHA_ACTUALIZACION DATETIME NULL,
    ID_USUARIO_REGISTRO INT NULL,
    ID_USUARIO_ACTUALIZA INT NULL,
    -- Foreign Keys
    CONSTRAINT FK_Atenciones_TipoTramite FOREIGN KEY (ID_TIPO_TRAMITE) 
        REFERENCES UTD_TIPO_TRAMITE(ID_TIPO_TRAMITE),
    CONSTRAINT FK_Atenciones_TipoPreferencial FOREIGN KEY (ID_TIPO_PREFERENCIAL) 
        REFERENCES UTD_TIPO_PREFERENCIAL(ID_TIPO_PREFERENCIAL),
    CONSTRAINT FK_Atenciones_EstadoAtencion FOREIGN KEY (ID_ESTADO_ATENCION) 
        REFERENCES UTD_ESTADO_ATENCION(ID_ESTADO_ATENCION),
    CONSTRAINT FK_Atenciones_UsuarioRegistro FOREIGN KEY (ID_USUARIO_REGISTRO) 
        REFERENCES ADM_Usuario(ID_USUARIO),
    CONSTRAINT FK_Atenciones_UsuarioActualiza FOREIGN KEY (ID_USUARIO_ACTUALIZA) 
        REFERENCES ADM_Usuario(ID_USUARIO)
);

-- Índices para mejorar el rendimiento
CREATE INDEX IX_Atenciones_Estado ON UTD_ATENCIONES(ID_ESTADO_ATENCION);
CREATE INDEX IX_Atenciones_FechaRegistro ON UTD_ATENCIONES(FECHA_REGISTRO DESC);
CREATE INDEX IX_Atenciones_NumeroDocumento ON UTD_ATENCIONES(NUMERO_DOCUMENTO);
CREATE INDEX IX_Atenciones_TipoTramite ON UTD_ATENCIONES(ID_TIPO_TRAMITE);

GO

-- =============================================
-- Datos Iniciales: Estados
-- =============================================
INSERT INTO UTD_ESTADO_ATENCION (NOMBRE_ESTADO, ORDEN, DESCRIPCION) VALUES
('Pendiente', 1, 'Atención registrada, esperando ser llamada'),
('En Ventanilla', 2, 'Atención siendo atendida en ventanilla'),
('Atendido', 3, 'Atención finalizada correctamente');

-- =============================================
-- Datos Iniciales: Tipos de Trámite
-- =============================================
INSERT INTO UTD_TIPO_TRAMITE (NOMBRE_TRAMITE, DESCRIPCION) VALUES
('Mesa de Partes', 'Solo para el ingreso de documentos dirigidos a la DIGESA.'),
('Informes', 'Solo para consultas, seguimiento de trámites, información, recojo de documentos.');

-- =============================================
-- Datos Iniciales: Tipos Preferencial
-- =============================================
INSERT INTO UTD_TIPO_PREFERENCIAL (NOMBRE_TIPO_PREFERENCIAL, DESCRIPCION) VALUES
('Adulto Mayor', 'Personas mayores de 60 años'),
('Gestante', 'Mujeres en estado de gestación'),
('Discapacidad', 'Personas con discapacidad física o mental'),
('Niño', 'Menores de edad (0-11 años)'),
('Persona con Bebé', 'Personas acompañadas de bebés menores de 1 año');

GO

-- =============================================
-- Vista para consultas comunes
-- =============================================
CREATE VIEW VW_ATENCIONES_DETALLE AS
SELECT 
    a.ID_ATENCION,
    a.TIPO_DOCUMENTO,
    a.NUMERO_DOCUMENTO,
    a.NOMBRES,
    a.APELLIDOS,
    a.NOMBRES + ' ' + a.APELLIDOS AS NOMBRE_COMPLETO,
    tt.NOMBRE_TRAMITE,
    tt.DESCRIPCION AS DESCRIPCION_TRAMITE,
    a.OBSERVACION,
    a.ES_PREFERENCIAL,
    tp.NOMBRE_TIPO_PREFERENCIAL,
    ea.NOMBRE_ESTADO,
    ea.ORDEN AS ORDEN_ESTADO,
    a.FECHA_REGISTRO,
    a.FECHA_ACTUALIZACION,
    ur.NombreUsuarioUSU AS USUARIO_REGISTRO,
    ua.NombreUsuarioUSU AS USUARIO_ACTUALIZA
FROM UTD_ATENCIONES a
INNER JOIN UTD_TIPO_TRAMITE tt ON a.ID_TIPO_TRAMITE = tt.ID_TIPO_TRAMITE
LEFT JOIN UTD_TIPO_PREFERENCIAL tp ON a.ID_TIPO_PREFERENCIAL = tp.ID_TIPO_PREFERENCIAL
INNER JOIN UTD_ESTADO_ATENCION ea ON a.ID_ESTADO_ATENCION = ea.ID_ESTADO_ATENCION
LEFT JOIN ADM_Usuario ur ON a.ID_USUARIO_REGISTRO = ur.ID_USUARIO
LEFT JOIN ADM_Usuario ua ON a.ID_USUARIO_ACTUALIZA = ua.ID_USUARIO;

GO

PRINT 'Tablas de Atenciones creadas exitosamente';
