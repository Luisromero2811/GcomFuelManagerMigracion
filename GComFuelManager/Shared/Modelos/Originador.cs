using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GComFuelManager.Shared.Modelos
{
    public class Originador
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        [NotMapped, JsonIgnore] public List<Vendedor_Originador> Vendedor_Originador { get; set; } = new();
        [NotMapped] public List<Vendedor> Vendedores { get; set; } = new();
    }
}
