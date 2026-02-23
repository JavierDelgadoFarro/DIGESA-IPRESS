-- =============================================
-- Script: 03_Crear_Tabla_Historial_Atenciones.sql
-- Descripción: Crea la tabla para registrar el historial de cambios de estado
--              de cada atención, permitiendo rastrear quién, cuándo y qué cambió
-- Fecha: 2026-02-22
-- =============================================

-- =============================================
-- Crear tabla de historial de atenciones
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UTD_HISTORIAL_ATENCIONES]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UTD_HISTORIAL_ATENCIONES](
        [ID_HISTORIAL] [int] IDENTITY(1,1) NOT NULL,
        [ID_ATENCION] [int] NOT NULL,
        [ID_ESTADO_ANTERIOR] [int] NULL,
        [ID_ESTADO_NUEVO] [int] NOT NULL,
        [ID_USUARIO] [int] NULL,
        [FECHA_CAMBIO] [datetime] NOT NULL DEFAULT GETDATE(),
        [OBSERVACION] [nvarchar](1000) NULL,
        [TIEMPO_EN_ESTADO_ANTERIOR] [int] NULL, -- Tiempo en minutos que estuvo en el estado anterior
        CONSTRAINT [PK_UTD_HISTORIAL_ATENCIONES] PRIMARY KEY CLUSTERED ([ID_HISTORIAL] ASC),
        CONSTRAINT [FK_HISTORIAL_ATENCION] FOREIGN KEY([ID_ATENCION]) 
            REFERENCES [dbo].[UTD_ATENCIONES] ([ID_ATENCION]),
        CONSTRAINT [FK_HISTORIAL_ESTADO_ANTERIOR] FOREIGN KEY([ID_ESTADO_ANTERIOR]) 
            REFERENCES [dbo].[UTD_ESTADO_ATENCION] ([ID_ESTADO_ATENCION]),
        CONSTRAINT [FK_HISTORIAL_ESTADO_NUEVO] FOREIGN KEY([ID_ESTADO_NUEVO]) 
            REFERENCES [dbo].[UTD_ESTADO_ATENCION] ([ID_ESTADO_ATENCION]),
        CONSTRAINT [FK_HISTORIAL_USUARIO] FOREIGN KEY([ID_USUARIO]) 
            REFERENCES [dbo].[ADM_Usuario] ([ID_Usuario])
    )
    
    PRINT 'Tabla UTD_HISTORIAL_ATENCIONES creada exitosamente.'
END
ELSE
BEGIN
    PRINT 'La tabla UTD_HISTORIAL_ATENCIONES ya existe.'
END
GO

-- =============================================
-- Crear índices para mejorar el rendimiento
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HISTORIAL_ATENCION' AND object_id = OBJECT_ID('UTD_HISTORIAL_ATENCIONES'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_HISTORIAL_ATENCION] ON [dbo].[UTD_HISTORIAL_ATENCIONES]
    (
        [ID_ATENCION] ASC,
        [FECHA_CAMBIO] ASC
    )
    PRINT 'Índice IX_HISTORIAL_ATENCION creado exitosamente.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HISTORIAL_ESTADO' AND object_id = OBJECT_ID('UTD_HISTORIAL_ATENCIONES'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_HISTORIAL_ESTADO] ON [dbo].[UTD_HISTORIAL_ATENCIONES]
    (
        [ID_ESTADO_NUEVO] ASC,
        [FECHA_CAMBIO] DESC
    )
    PRINT 'Índice IX_HISTORIAL_ESTADO creado exitosamente.'
END
GO

-- =============================================
-- Insertar registros históricos para atenciones existentes
-- =============================================
-- Si ya tienes atenciones creadas, este script creará un registro inicial
-- para cada una en su estado actual

IF EXISTS (SELECT 1 FROM UTD_ATENCIONES)
BEGIN
    INSERT INTO [dbo].[UTD_HISTORIAL_ATENCIONES] 
    (
        [ID_ATENCION],
        [ID_ESTADO_ANTERIOR],
        [ID_ESTADO_NUEVO],
        [ID_USUARIO],
        [FECHA_CAMBIO],
        [OBSERVACION]
    )
    SELECT 
        a.ID_ATENCION,
        NULL, -- No hay estado anterior para registros existentes
        a.ID_ESTADO_ATENCION,
        a.ID_USUARIO_REGISTRO,
        a.FECHA_REGISTRO,
        'Registro inicial migrado al historial'
    FROM [dbo].[UTD_ATENCIONES] a
    WHERE NOT EXISTS (
        SELECT 1 
        FROM [dbo].[UTD_HISTORIAL_ATENCIONES] h 
        WHERE h.ID_ATENCION = a.ID_ATENCION
    )
    
    PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' registros históricos iniciales creados para atenciones existentes.'
END
GO

-- =============================================
-- Crear vista para consultar el historial con datos completos
-- =============================================
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VW_HISTORIAL_ATENCIONES_DETALLE')
    DROP VIEW [dbo].[VW_HISTORIAL_ATENCIONES_DETALLE]
GO

CREATE VIEW [dbo].[VW_HISTORIAL_ATENCIONES_DETALLE]
AS
SELECT 
    h.ID_HISTORIAL,
    h.ID_ATENCION,
    h.ID_ESTADO_ANTERIOR,
    ea.NOMBRE_ESTADO as NOMBRE_ESTADO_ANTERIOR,
    h.ID_ESTADO_NUEVO,
    en.NOMBRE_ESTADO as NOMBRE_ESTADO_NUEVO,
    en.ORDEN as ORDEN_ESTADO_NUEVO,
    h.ID_USUARIO,
    CASE 
        WHEN p.Nombre IS NOT NULL THEN 
            LTRIM(RTRIM(p.Nombre)) + ' ' + 
            LTRIM(RTRIM(ISNULL(p.Paterno, ''))) + ' ' + 
            LTRIM(RTRIM(ISNULL(p.Materno, '')))
        ELSE NULL 
    END as NOMBRE_USUARIO,
    h.FECHA_CAMBIO,
    h.OBSERVACION,
    h.TIEMPO_EN_ESTADO_ANTERIOR,
    -- Calcular tiempo hasta el siguiente cambio (NULL si es el último)
    DATEDIFF(MINUTE, h.FECHA_CAMBIO, 
        (SELECT MIN(h2.FECHA_CAMBIO) 
         FROM UTD_HISTORIAL_ATENCIONES h2 
         WHERE h2.ID_ATENCION = h.ID_ATENCION 
         AND h2.FECHA_CAMBIO > h.FECHA_CAMBIO)
    ) as MINUTOS_EN_ESTE_ESTADO
FROM [dbo].[UTD_HISTORIAL_ATENCIONES] h
LEFT JOIN [dbo].[UTD_ESTADO_ATENCION] ea ON h.ID_ESTADO_ANTERIOR = ea.ID_ESTADO_ATENCION
INNER JOIN [dbo].[UTD_ESTADO_ATENCION] en ON h.ID_ESTADO_NUEVO = en.ID_ESTADO_ATENCION
LEFT JOIN [dbo].[ADM_Usuario] u ON h.ID_USUARIO = u.ID_Usuario
LEFT JOIN [dbo].[ADM_Personal] p ON u.ID_Personal = p.ID_Personal
GO

PRINT 'Vista VW_HISTORIAL_ATENCIONES_DETALLE creada exitosamente.'
GO

-- =============================================
-- Script de prueba (comentado)
-- =============================================
/*
-- Ver el historial de una atención específica
SELECT * 
FROM VW_HISTORIAL_ATENCIONES_DETALLE 
WHERE ID_ATENCION = 1
ORDER BY FECHA_CAMBIO

-- Ver todas las atenciones que están actualmente en un estado específico
SELECT a.*, h.*
FROM UTD_ATENCIONES a
INNER JOIN VW_HISTORIAL_ATENCIONES_DETALLE h ON a.ID_ATENCION = h.ID_ATENCION
WHERE h.ID_ESTADO_NUEVO = 1 -- Pendiente
AND NOT EXISTS (
    SELECT 1 FROM UTD_HISTORIAL_ATENCIONES h2 
    WHERE h2.ID_ATENCION = h.ID_ATENCION 
    AND h2.FECHA_CAMBIO > h.FECHA_CAMBIO
)
*/
