using System.ComponentModel.DataAnnotations;

namespace GComFuelManager.Shared.Modelos
{
    public class Moneda
    {
        public int? Id { get; set; }
        [Required(AllowEmptyStrings = true, ErrorMessage = "No se permite valor vacio"), MaxLength(15)]
        public string Nombre { get; set; } = string.Empty;
        public bool Estatus { get; set; } = true;
    }
}
