using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GComFuelManager.Shared.Modelos
{
    public class Redireccionamiento
    {
        public int Id { get; set; }
        public Int64 Id_Orden { get; set; }

        [Validar_Cero(ErrorMessageResourceName = "Grupo"), DisplayName("Grupo")]
        public Int16 Grupo_Red { get; set; } = 0;

        [Validar_Cero(ErrorMessageResourceName = "Cliente"), DisplayName("Cliente")]
        public int Cliente_Red { get; set; } = 0;

        [Validar_Cero(ErrorMessageResourceName = "Destino"), DisplayName("Destino")]
        public int Destino_Red { get; set; } = 0;

        [Validar_Cero(ErrorMessageResourceName = "Precio"), Validar_Negativos, DisplayName("Precio")]
        public double Precio_Red { get; set; }

        [Validar_Longitud(ErrorMessageResourceName = "Motivo"), DisplayName("Motivo")]
        public string Motivo_Red { get; set; } = string.Empty;
        //[Required(ErrorMessageResourceName = "Fecha de redireccion", ErrorMessage = "{0} no tiene un valor valido.")]
        public DateTime Fecha_Red { get; set; } = DateTime.Today;
        public DateTime Fecha { get; set; } = DateTime.Now;


        public Orden? Orden { get; set; } = null!;
        public Grupo? Grupo { get; set; } = null!;
        public Cliente? Cliente { get; set; } = null!;
        public Destino? Destino { get; set; } = null!;

        public string Nombre_Grupo
        {
            get
            {
                if (Grupo is not null)
                    if (!string.IsNullOrEmpty(Grupo.Den))
                        return Grupo.Den;

                return string.Empty;
            }
        }
        public string Nombre_Cliente
        {
            get
            {
                if (Cliente is not null)
                    if (!string.IsNullOrEmpty(Cliente.Den))
                        return Cliente.Den;

                return string.Empty;
            }
        }
        public string Nombre_Cliente_Origibal
        {
            get
            {
                if (Orden is not null)
                    return Orden.Obtener_Cliente_De_Orden;

                return string.Empty;
            }
        }
        public string Nombre_Destino
        {
            get
            {
                if (Destino is not null)
                    if (!string.IsNullOrEmpty(Destino.Den))
                        return Destino.Den;

                return string.Empty;
            }
        }
        public string Nombre_Destino_Original
        {
            get
            {
                if (Orden is not null)
                    return Orden.Obtener_Destino_De_Orden;
                return string.Empty;
            }
        }
        public int Bol_Orden
        {
            get
            {
                if (Orden is not null)
                    if (Orden.BatchId is not null)
                        return (int)Orden.BatchId;
                return 0;
            }
        }
    }

    public class Redireccion_Excel
    {
        public int BOL { get; set; }
        public string Producto { get; set; } = string.Empty;
        [DisplayName("Cliente original")]
        public string Cliente_Original { get; set; } = string.Empty;
        [DisplayName("Cliente de redireccion")]
        public string Cliente_Red { get; set; } = string.Empty;
        [DisplayName("Destino original")]
        public string Destino_Original { get; set; } = string.Empty;
        [DisplayName("Destino de redireccion")]
        public string Destino_Red { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        [DisplayName("Fecha de redireccion")]
        public string Fecha_Redireccion { get; set; } = string.Empty;
    }

    public class Validar_Longitud : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validation)
        {
            var displayName = validation.ObjectType.GetProperty(validation.MemberName).GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;

            var name = displayName?.DisplayName ?? validation.DisplayName;

            if (value is null || string.IsNullOrEmpty(value.ToString()) || string.IsNullOrWhiteSpace(value.ToString())) return new ValidationResult($"{name} no acepta valores vacios");

            if (value is not null && !string.IsNullOrEmpty(value.ToString()) && value?.ToString()?.Length > 250) return new ValidationResult($"{name} no acepta mas de 250 caracteres");

            return ValidationResult.Success!;

        }
    }
}
