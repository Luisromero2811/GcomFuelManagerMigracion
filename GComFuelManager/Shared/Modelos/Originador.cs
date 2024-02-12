using OfficeOpenXml.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GComFuelManager.Shared.Modelos
{
    public class Originador
    {
        public int Id { get; set; }
        [StringLength(250,ErrorMessage = "{0} no debe de tener una longitud de mas de 250 caracteres")] public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        [NotMapped, JsonIgnore] public List<Vendedor_Originador> Vendedor_Originador { get; set; } = new();
        [NotMapped] public List<Vendedor> Vendedores { get; set; } = new();
        [NotMapped] public List<Cliente> Clientes { get; set; } = new();
        [NotMapped] public List<Mes_Venta> Venta_Por_Meses { get; set; } = new();
    }
}
