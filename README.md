# DIGESA-IPRESS

Sistema de gestión para DIGESA-IPRESS. Proyecto base limpio y escalable.

## Arquitectura

- **VisitasTickets (API)**: ASP.NET Core 8 Web API con JWT
- **VisitasTickets.Client**: Blazor WebAssembly
- **VisitasTickets.Domain**: Entidades y DTOs
- **VisitasTickets.Application**: Servicios de negocio (IAuthService)
- **VisitasTickets.Infrastructure**: EF Core, DbContext

## Configuración

### IP del servidor
Editar `VisitasTickets.Client/Globals/AppConfig.cs`:
```csharp
public const string ApiBaseUrl = "https://192.168.1.142:7248";
```

### Base de datos
1. Crear base de datos `DIGESA_IPRESS`
2. Ejecutar `Scripts/00_DIGESA_IPRESS_Completo.sql`
3. Configurar cadena de conexión en `appsettings.json` o variables de entorno

### Credenciales iniciales
- Usuario: `admin`
- Contraseña: `admin` (cambiar en primer acceso)

## Ejecución

```bash
# API
cd VisitasTickets && dotnet run

# Cliente (en otra terminal)
cd VisitasTickets.Client && dotnet run
```

## Repositorio
https://github.com/JavierDelgadoFarro/DIGESA-IPRESS
