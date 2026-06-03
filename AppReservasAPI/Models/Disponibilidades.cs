using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppReservasAPI.Models
{
    [Table("Disponibilidades", Schema = "viajes")]

    public class Disponibilidades
    {
        [Key]
        public int DisponibilidadId { get; set; }

        public int ViajeId { get; set; }

        public DateTime Fecha { get; set; }

        public int CuposTotales { get; set; }

        public int CuposDisponibles { get; set; }

        [ForeignKey (nameof(ViajeId))]
        public Viaje? viaje { get; set; }



    }
}
