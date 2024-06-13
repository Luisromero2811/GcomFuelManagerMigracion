using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
namespace GComFuelManager.Shared.Modelos
{
    public class Producto
    {
        //5
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte Cod { get; set; }//FK
        [MaxLength(50)]
        public string? Den { get; set; } = string.Empty;
        [MaxLength(10)]
        public string? Codsyn { get; set; } = string.Empty;
        public bool? Activo { get; set; } = true;
        public short? Id_Tad { get; set; } = 0;
        public short? Id_Tipo { get; set; } = 0;
        [DisplayName("Identificador externo"), StringLength(50)]
        public string? Identificador_Externo { get; set; } = string.Empty;

        public string Nombre_Producto { get { return !string.IsNullOrEmpty(Den) ? Den : string.Empty; } }
        public string Obtener_Tipo
        {
            get
            {
                if (TipoProducto is not null)
                    return TipoProducto.Tipo;

                return string.Empty;
            }
        }

        public string Obtener_Terminal
        {
            get
            {
                if (Terminal is not null)
                    if (!string.IsNullOrEmpty(Terminal.Den) || !string.IsNullOrWhiteSpace(Terminal.Den))
                        return Terminal.Den;

                return string.Empty;
            }
        }

        public TipoProducto? TipoProducto { get; set; } = null!;
        public Tad? Terminal { get; set; } = null!;

        public Producto HardCopy()
        {
            return new()
            {
                Cod = Cod,
                Den = Den,
                Codsyn = Codsyn,
                Activo = Activo,
                Id_Tad = Id_Tad,
                Id_Tipo = Id_Tipo,
            };
        }
    }
}

