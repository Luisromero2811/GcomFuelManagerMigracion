using System.ComponentModel.DataAnnotations;

namespace GComFuelManager.Shared.Modelos
{
    public class Redireccionamiento
    {
        public int Id { get; set; }
        public Int64 Id_Orden { get; set; } = 0;
        public Int16 Grupo_Red { get; set; } = 0;
        public int Cliente_Red { get; set; } = 0;
        public int Destino_Red { get; set; } = 0;
        [Validar_Cero, Validar_Negativos]
        public double Precio_Red { get; set; } = 0;
        [Required(AllowEmptyStrings = false), StringLength(250)]
        public string Motivo_Red { get; set; } = string.Empty;
        public DateTime Fecha_Red { get; set; } = DateTime.Today;
        public DateTime Fecha { get; set; } = DateTime.Now;


        public Orden Orden { get; set; } = new();
        public Grupo Grupo { get; set; } = new();
        public Cliente Cliente { get; set; } = new();
        public Destino Destino { get; set; } = new();
    }
}
