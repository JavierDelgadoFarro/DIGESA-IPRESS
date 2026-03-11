using System.ComponentModel.DataAnnotations.Schema;

namespace IPRESS.Domain.Entities
{
    /// <summary>Departamento del Perú (código 2 caracteres, ej. 15 = LIMA).</summary>
    public class IpressDepartamento
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public virtual ICollection<IpressProvincia> Provincias { get; set; } = new List<IpressProvincia>();
    }

    /// <summary>Provincia del Perú (código 4 caracteres = dep+prov, ej. 1501 = LIMA).</summary>
    public class IpressProvincia
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string CodigoDepartamento { get; set; } = "";
        public virtual IpressDepartamento? Departamento { get; set; }
        public virtual ICollection<IpressDistrito> Distritos { get; set; } = new List<IpressDistrito>();
    }

    /// <summary>Distrito del Perú (ubigeo 6 caracteres, ej. 150101 = LIMA).</summary>
    public class IpressDistrito
    {
        public string Ubigeo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string CodigoProvincia { get; set; } = "";
        public virtual IpressProvincia? Provincia { get; set; }
    }

    /// <summary>DIRESA: código numérico, nombre, ubicación (departamento, provincia, distrito, ubigeo).</summary>
    public class IpressDiresa
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdDiresa { get; set; }
        public int Codigo { get; set; }
        public string Nombre { get; set; } = "";
        /// <summary>Ubigeo 6 caracteres; FK a IPRESS_Distrito. Departamento/Provincia/Distrito se obtienen por join.</summary>
        public string? Ubigeo { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime? FechaCreacion { get; set; }

        public virtual IpressDistrito? Distrito { get; set; }
        public virtual ICollection<IpressRed> Redes { get; set; } = new List<IpressRed>();
    }

    /// <summary>RED: código numérico, microred (nombre), Diresa, ubicación propia (puede ser distinta a la Diresa).</summary>
    public class IpressRed
    {
        public int IdRed { get; set; }
        public int IdDiresa { get; set; }
        public int Codigo { get; set; }
        public string Nombre { get; set; } = "";
        public string? Ubigeo { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime? FechaCreacion { get; set; }

        public virtual IpressDiresa? Diresa { get; set; }
        public virtual IpressDistrito? Distrito { get; set; }
        public virtual ICollection<IpressMicroRed> MicroRedes { get; set; } = new List<IpressMicroRed>();
    }

    /// <summary>MICRORED: código numérico, nombre, Red, ubicación propia (puede ser distinta a la Red).</summary>
    public class IpressMicroRed
    {
        public int IdMicroRed { get; set; }
        public int IdRed { get; set; }
        public int Codigo { get; set; }
        public string Nombre { get; set; } = "";
        public string? Ubigeo { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime? FechaCreacion { get; set; }

        public virtual IpressRed? Red { get; set; }
        public virtual IpressDistrito? Distrito { get; set; }
    }

    public class IpressCentroPoblado
    {
        public int IdCentroPoblado { get; set; }
        /// <summary>Ubigeo de 6 caracteres derivado de Departamento/Provincia/Distrito (solo lectura en formulario).</summary>
        public string? Ubigeo { get; set; }
        public string UbigeoCcp { get; set; } = "";
        public string? Departamento { get; set; }
        public string? Provincia { get; set; }
        public string? Distrito { get; set; }
        public string? CentroPoblado { get; set; }
        public int? IdEstablecimiento { get; set; }
        public string? Ambito { get; set; }
        public string? Quintil { get; set; }
        public decimal? Este { get; set; }
        public decimal? Norte { get; set; }
        public int? Huso { get; set; }
        public string? Banda { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public int? AltitudMsnm { get; set; }
        public int? PoblacionTotal { get; set; }
        public int? PoblacionServida { get; set; }
        public int? PoblacionVigilada { get; set; }
        public int? ElectricidadHrs { get; set; }
        public string? TelefonoTipo { get; set; }
        public string? TelefonoNumero { get; set; }
        public bool RadioEmisora { get; set; }
        public bool RadioESS { get; set; }
        public bool SenalTV { get; set; }
        public bool Internet { get; set; }
        public bool LimpiezaPublica { get; set; }
        public bool Agua { get; set; }
        public bool Letrinas { get; set; }
        public bool DesagueAlcantarillado { get; set; }
        public bool SistEliminacionExcretas { get; set; }
        public string? Vertimientos { get; set; }
        public decimal? TempMinima { get; set; }
        public decimal? TempMaxima { get; set; }
        public bool Activo { get; set; } = true;

        public virtual IpressEstablecimiento? Establecimiento { get; set; }
        public virtual ICollection<IpressCentroPobladoAccesibilidad> Accesibilidades { get; set; } = new List<IpressCentroPobladoAccesibilidad>();
        public virtual ICollection<IpressCentroPobladoCentroEducativo> CentrosEducativos { get; set; } = new List<IpressCentroPobladoCentroEducativo>();
        public virtual ICollection<IpressCentroPobladoAutoridad> Autoridades { get; set; } = new List<IpressCentroPobladoAutoridad>();
    }

    public class IpressCentroPobladoAccesibilidad
    {
        public int IdAccesibilidad { get; set; }
        public int IdCentroPoblado { get; set; }
        public string? Desde { get; set; }
        public string? Hasta { get; set; }
        public decimal? DistanciaKm { get; set; }
        public int? TiempoMin { get; set; }
        public string? TipoVia { get; set; }
        public string? MedioTransporte { get; set; }
        public virtual IpressCentroPoblado? CentroPoblado { get; set; }
    }

    public class IpressCentroPobladoCentroEducativo
    {
        public int IdCentroEducativo { get; set; }
        public int IdCentroPoblado { get; set; }
        public string? TipoCentroEducativo { get; set; }
        public string? NombreCentroEducativo { get; set; }
        public virtual IpressCentroPoblado? CentroPoblado { get; set; }
    }

    public class IpressCentroPobladoAutoridad
    {
        public int IdAutoridad { get; set; }
        public int IdCentroPoblado { get; set; }
        public string? TipoAutoridad { get; set; }
        public string? NombreAutoridad { get; set; }
        public virtual IpressCentroPoblado? CentroPoblado { get; set; }
    }

    public class IpressEstablecimiento
    {
        public int IdEstablecimiento { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Departamento { get; set; }
        public string? Provincia { get; set; }
        public string? Distrito { get; set; }
        public string? QuintilRegional { get; set; }
        public string? Ubigeo { get; set; }
        public int? AltitudMsnm { get; set; }
        public int? IdDiresa { get; set; }
        public int? IdRed { get; set; }
        public int? IdMicroRed { get; set; }
        public bool TieneTelefono { get; set; }
        public bool TieneRadio { get; set; }
        public decimal? Este { get; set; }
        public decimal? Norte { get; set; }
        public int? Huso { get; set; }
        public string? Banda { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime? FechaCreacion { get; set; }

        public virtual IpressDiresa? Diresa { get; set; }
        public virtual IpressRed? Red { get; set; }
        public virtual IpressMicroRed? MicroRed { get; set; }
        public virtual ICollection<IpressEstablecimientoCentroPoblado> EstablecimientoCentrosPoblados { get; set; } = new List<IpressEstablecimientoCentroPoblado>();
    }

    public class IpressEstablecimientoCentroPoblado
    {
        public int IdEstablecimiento { get; set; }
        public int IdCentroPoblado { get; set; }
        public virtual IpressEstablecimiento? Establecimiento { get; set; }
        public virtual IpressCentroPoblado? CentroPoblado { get; set; }
    }
}
