# Scripts IPRESS - Base de datos

## Orden de ejecución

### Instalación nueva (BD vacía)
1. **IPRESS_DDL.sql** — Crea la base de datos, tablas, índices, procedimientos, funciones y triggers. Al inicio elimina tablas legacy **ADM_** (ADM_Area, ADM_Detalle_Sub_Menu, ADM_Menu, ADM_Modulo, ADM_Personal, ADM_Sub_Menu, ADM_Usuario) si existieran; el proyecto solo usa tablas con prefijo **IPRESS_**.
2. **IPRESS_Inserts.sql** — Inserta datos iniciales (módulos, roles, usuario admin, menús, usuarios de prueba).

### Opcional: Ubigeo completo de Perú
3. **Insert_Ubigeo_Peru_Completo.sql** — Carga departamentos, provincias y distritos de todo el país (después de Inserts).

### Solo vaciar y repoblar datos
1. **IPRESS_Limpieza.sql** — Vacía todas las tablas (respeta estructura).
2. **IPRESS_Inserts.sql** — Vuelve a insertar datos iniciales.

---

**Resumen:** Para una instalación desde cero ejecutar en este orden: **IPRESS_DDL.sql** → **IPRESS_Inserts.sql** (y opcionalmente **Insert_Ubigeo_Peru_Completo.sql**).

---

## Contraseñas y columnas

- En base de datos la columna de contraseña se llama **Password** (tabla `IPRESS_Usuario`).
- Las contraseñas se almacenan **hasheadas** con BCrypt. No existe desencriptación: el login **verifica** con `BCrypt.Verify`. La contraseña inicial de `admin` en los inserts es "admin"; la aplicación la hashea en el **primer inicio de sesión**.

## Procedimientos almacenados

- **SP_ValidarUsuario** @Usuario, @Password: devuelve usuario por nombre (la app verifica el hash con BCrypt).
- **SP_Usuario_ActualizarPassword** @IdUsuario, @PasswordHash: actualiza la contraseña (usado en “Cambiar contraseña”).
- **SP_Auditoria_Registrar**: registro explícito en `IPRESS_Auditoria` con UsuarioId e IpOrigen.

## Auditoría y trazabilidad

- En `IPRESS_Auditoria` se guardan **UsuarioId** e **IpOrigen** en cada registro (triggers y CONTEXT_INFO desde la app).
- La trazabilidad de quién crea, edita o elimina se obtiene de los registros en `IPRESS_Auditoria` (UsuarioId + Tabla + Accion).

## Error "Duplicate key" en IPRESS_Auditoria al importar masivamente

Si al importar Red (o otros módulos) aparece *Violation of PRIMARY KEY constraint ... Cannot insert duplicate key in object 'dbo.IPRESS_Auditoria'. The duplicate key value is (N)*, el **IDENTITY** de `IPRESS_Auditoria` está desincronizado. Solución:

1. **Ejecutar de nuevo IPRESS_DDL.sql** (al final reajusta el identity de Auditoría), o  
2. **Ejecutar solo este bloque en la BD IPRESS:**
   ```sql
   USE IPRESS;
   DECLARE @maxAudit BIGINT = (SELECT ISNULL(MAX(ID_AUDITORIA), 0) FROM IPRESS_Auditoria);
   DBCC CHECKIDENT ('IPRESS_Auditoria', RESEED, @maxAudit);
   ```
   Luego vuelva a intentar la importación.
