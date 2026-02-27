-- =============================================
-- Script: Agregar Estado "Cancelado" para Atenciones
-- Descripción: Permite cancelar atenciones cuando la persona no está presente
-- Fecha: 2026-02-26
-- =============================================

-- Insertar el nuevo estado "Cancelado" con orden 5
IF NOT EXISTS (SELECT 1 FROM UTD_ESTADO_ATENCION WHERE NOMBRE_ESTADO = 'Cancelado')
BEGIN
    INSERT INTO UTD_ESTADO_ATENCION (NOMBRE_ESTADO, ORDEN, DESCRIPCION) 
    VALUES ('Cancelado', 5, 'Atención cancelada - Persona no presente');
    
    PRINT 'Estado "Cancelado" agregado correctamente con ORDEN = 5';
END
ELSE
BEGIN
    PRINT 'El estado "Cancelado" ya existe en la base de datos';
END
GO

-- Verificar los estados actuales
SELECT 
    ID_ESTADO_ATENCION,
    NOMBRE_ESTADO,
    ORDEN,
    DESCRIPCION
FROM UTD_ESTADO_ATENCION
ORDER BY ORDEN;
GO
