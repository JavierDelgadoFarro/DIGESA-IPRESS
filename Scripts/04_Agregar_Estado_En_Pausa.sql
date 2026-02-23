-- =============================================
-- Script: Agregar Estado "En Pausa" para Atenciones
-- Descripción: Permite pausar/reanudar atenciones sin contabilizar tiempo de pausa
-- Fecha: 2026-02-23
-- =============================================

-- Primero, actualizar el orden del estado "Atendido" de 3 a 4
UPDATE UTD_ESTADO_ATENCION
SET ORDEN = 4
WHERE NOMBRE_ESTADO = 'Atendido';
GO

-- Insertar el nuevo estado "En Pausa" con orden 3
IF NOT EXISTS (SELECT 1 FROM UTD_ESTADO_ATENCION WHERE NOMBRE_ESTADO = 'En Pausa')
BEGIN
    INSERT INTO UTD_ESTADO_ATENCION (NOMBRE_ESTADO, ORDEN, DESCRIPCION) 
    VALUES ('En Pausa', 3, 'Atención pausada temporalmente, no contabiliza tiempo');
    
    PRINT 'Estado "En Pausa" agregado correctamente con ORDEN = 3';
END
ELSE
BEGIN
    PRINT 'El estado "En Pausa" ya existe en la base de datos';
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
