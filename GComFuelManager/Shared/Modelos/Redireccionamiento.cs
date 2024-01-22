using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GComFuelManager.Shared.Modelos
{
    public class Redireccionamiento
    {
        public int Id { get; set; }
        public Int64 Id_Orden { get; set; }

        [Validar_Cero(ErrorMessageResourceName = "Grupo")]
        public Int16 Grupo_Red { get; set; } = 0;

        [Validar_Cero(ErrorMessageResourceName = "Cliente"), DisplayName("Cliente")]
        public int Cliente_Red { get; set; } = 0;

        [Validar_Cero(ErrorMessageResourceName = "Destino"), DisplayName("Destino")]
        public int Destino_Red { get; set; } = 0;

        [Validar_Cero(ErrorMessageResourceName = "Precio"), Validar_Negativos]
        public double Precio_Red { get; set; }

        [Validar_Longitud(ErrorMessageResourceName ="Motivo"), DisplayName("Motivo")]
        public string Motivo_Red { get; set; } = string.Empty;
        //[Required(ErrorMessageResourceName = "Fecha de redireccion", ErrorMessage = "{0} no tiene un valor valido.")]
        public DateTime Fecha_Red { get; set; } = DateTime.Today;
        public DateTime Fecha { get; set; } = DateTime.Now;


        public Orden? Orden { get; set; } = null!;
        public Grupo? Grupo { get; set; } = null!;
        public Cliente? Cliente { get; set; } = null!;
        public Destino? Destino { get; set; } = null!;
    }

    public class Validar_Longitud : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validation)
        {
            if (value is null || string.IsNullOrEmpty(value.ToString()) || string.IsNullOrWhiteSpace(value.ToString())) return new ValidationResult($"{validation.DisplayName} no acepta valores vacios");

            if (value is not null && !string.IsNullOrEmpty(value.ToString()) && value?.ToString()?.Length > 250) return new ValidationResult($"{validation.DisplayName} no acepta mas de 250 caracteres");

            return ValidationResult.Success!;

        }
    }
}
