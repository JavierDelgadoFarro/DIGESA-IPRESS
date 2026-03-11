using IPRESS.Domain.Dtos;

namespace IPRESS.Client.Services
{
    /// <summary>
    /// Servicio para verificar permisos de botones por ruta.
    /// </summary>
    public class PermisosService
    {
        private readonly AuthService _authService;
        private AccesosResponse? _accesos;
        private Dictionary<string, HashSet<string>>? _botonesPorRuta;

        public PermisosService(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Obtiene los accesos actualizados (llamar tras login).
        /// </summary>
        public async Task CargarAccesosAsync()
        {
            _accesos = await _authService.GetAccesosAsync();
            _botonesPorRuta = null; // invalidar caché
        }

        /// <summary>
        /// Limpia el caché (llamar tras logout).
        /// </summary>
        public void Limpiar()
        {
            _accesos = null;
            _botonesPorRuta = null;
        }

        /// <summary>
        /// Comprueba si el usuario tiene permiso para el botón en la ruta indicada.
        /// </summary>
        /// <param name="ruta">Ruta actual, ej: /diresas, /red</param>
        /// <param name="codigoBoton">CREAR, EDITAR, ELIMINAR, IMPORTAR, EXPORTAR</param>
        public bool TienePermiso(string ruta, string codigoBoton)
        {
            var botones = ObtenerBotonesPorRuta();
            if (botones == null) return true; // por defecto permitir si no hay datos

            var rutaNorm = NormalizarRuta(ruta);
            if (botones.TryGetValue(rutaNorm, out var codigos))
                return codigos.Contains(codigoBoton, StringComparer.OrdinalIgnoreCase);

            // buscar por prefijo
            foreach (var kv in botones)
            {
                if (rutaNorm.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase))
                    return kv.Value.Contains(codigoBoton, StringComparer.OrdinalIgnoreCase);
            }
            return false;
        }

        private Dictionary<string, HashSet<string>>? ObtenerBotonesPorRuta()
        {
            if (_botonesPorRuta != null) return _botonesPorRuta;
            if (_accesos == null) return null;

            _botonesPorRuta = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var mod in _accesos.Modulos)
            {
                foreach (var menu in mod.Menus)
                {
                    foreach (var sub in menu.SubMenus)
                    {
                        var ruta = NormalizarRuta(sub.Ruta);
                        var set = new HashSet<string>(sub.Botones ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
                        if (!_botonesPorRuta.ContainsKey(ruta))
                            _botonesPorRuta[ruta] = set;
                        else
                            foreach (var b in set) _botonesPorRuta[ruta].Add(b);
                    }
                }
            }
            return _botonesPorRuta;
        }

        private static string NormalizarRuta(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta)) return "/";
            var r = ruta.Trim().ToLowerInvariant();
            if (!r.StartsWith("/")) r = "/" + r;
            return r;
        }

        /// <summary>
        /// Asigna los accesos cargados externamente (ej. desde MainLayout).
        /// </summary>
        public void SetAccesos(AccesosResponse? accesos)
        {
            _accesos = accesos;
            _botonesPorRuta = null;
        }
    }
}
