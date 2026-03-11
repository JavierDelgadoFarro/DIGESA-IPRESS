# IPRESS - Sistema de Gestión

Sistema de gestión para establecimientos de salud con maestros jerárquicos (Diresas, Red, MicroRed) y mantenimiento de establecimientos.

## Módulos

1. **Mantenimiento de Diresas** - CRUD + importación Excel
2. **Mantenimiento de Red** - Jerarquía sobre Diresas + importación Excel
3. **Mantenimiento de MicroRed** - Jerarquía sobre Red + importación Excel
4. **Mantenimiento de Establecimiento de Salud** - Con información principal, coordenadas UTM-WGS84 y centros poblados
5. **Gestión de Usuarios** - Roles, permisos por módulo y botón

## Base de datos

1. Crear base de datos `IPRESS` en SQL Server
2. Ejecutar `Scripts/IPRESS_BaseDeDatos_Completa.sql`
3. Credenciales iniciales: **admin** / **admin**

## Configuración

- **Conexión:** `appsettings.json` → `ConnectionStrings:DefaultConnection`
- **API URL (cliente):** El cliente usa la misma URL base donde se hospeda la API.

## Ejecución

```bash
# API (incluye cliente Blazor hospedado)
cd IPRESS.API && dotnet run
```

## Formato Excel para importación

Descargar el formato desde cada módulo (botón "Descargar formato") con las columnas requeridas antes de importar.

## Repositorio

https://github.com/JavierDelgadoFarro/DIGESA-IPRESS
