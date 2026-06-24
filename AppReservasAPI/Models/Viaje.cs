using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models;

[Table("Viajes", Schema = "viajes")]
public class Viaje
{
    [Key]
    public int ViajeId { get; set; }

    public int TipoViajeId { get; set; }

    public int DestinoId { get; set; }

    [Required]
    [StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Descripcion { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Precio { get; set; }

    public int CuposTotales { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    [StringLength(500)]
    public string? ImagenUrl { get; set; }

    public int? PublicadoPorUsuarioId { get; set; }

    [StringLength(30)]
    public string EstadoPublicacion { get; set; } = "Publicado";

    [ForeignKey(nameof(TipoViajeId))]
    public TipoViaje? TipoViaje { get; set; }

    [ForeignKey(nameof(DestinoId))]
    public Destino? Destino { get; set; }

    [ForeignKey(nameof(PublicadoPorUsuarioId))]
    public Usuario? PublicadoPorUsuario { get; set; }

    public ICollection<Disponibilidad> Disponibilidades { get; set; } = new List<Disponibilidad>();

    public ICollection<ViajeLike> Likes { get; set; } = new List<ViajeLike>();

    public ICollection<Queja> Quejas { get; set; } = new List<Queja>();

    public ICollection<Compartido> Compartidos { get; set; } = new List<Compartido>();
}
