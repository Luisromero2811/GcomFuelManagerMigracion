using System.Text.Json.Serialization;

namespace GComFuelManager.Shared.Modelos
{
    public class Consecutivo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Numeracion { get; set; } = 0;
        public short Id_Tad { get; set; } = 0;
        [JsonIgnore] public Tad? Terminal { get; set; } = null!;
        public string Obtener_Codigo_Terminal
        {
            get
            {
                if (Terminal is not null)
                    if (!string.IsNullOrEmpty(Terminal.Codigo))
                        return Terminal.Codigo;

                return string.Empty;
            }
        }
    }
}
