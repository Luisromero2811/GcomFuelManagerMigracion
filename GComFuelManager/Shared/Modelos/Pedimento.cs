using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace GComFuelManager.Shared.Modelos
{
    public class Pedimento
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "{0} no puede estar vacio", ErrorMessageResourceName = "Numero de pedimento"), StringLength(20), DisplayName("Numero de pedimento")]
        public string Numero_Pedimento { get; set; } = string.Empty;
        [Required(ErrorMessage = "{0} no puede estar vacio"), StringLength(14)]
        public string Referencia { get; set; } = string.Empty;
        [Required(ErrorMessage = "{0} no puede estar vacio"), StringLength(13), DisplayName("RFC"), Validar_RFC(ErrorMessage = "El RFC no tiene un valor valido", ErrorMessageResourceName = "RFC")]
        public string RFC_Exportador { get; set; } = string.Empty;
        [Required(ErrorMessage = "{0} no puede estar vacio", ErrorMessageResourceName = "Producto"), Validar_Cero, DisplayName("Producto")]
        public byte ID_Producto { get; set; } = 0;
        public DateTime Fecha_Actual { get; set; } = DateTime.Today;
        public DateTime Fecha_Creacion { get; set; } = DateTime.Now;
        public bool Activo { get; set; } = true;
        [Required(ErrorMessage = "{0} no puede estar vacio")]
        public double Costo { get; set; } = 0;
        [NotMapped]
        public Producto? Producto { get; set; } = null!;
        [NotMapped]
        public List<OrdenEmbarque> Ordens { get; set; } = new();
        [NotMapped]
        public double Litros_Totales { get; set; } = 0;
        [NotMapped]
        public double Utilidad { get; set; } = 0;
    }

    #region validadores
    public class Validar_Cero : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validation)
        {
            if (value.Equals(0))
                return new ValidationResult("El valor no es valido");

            return ValidationResult.Success!;
        }
    }

    public class Validar_RFC : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is null || string.IsNullOrEmpty(value.ToString()) || string.IsNullOrWhiteSpace(value.ToString())) return false;

            string Formato = @"^[A-Z]{4}\d{6}[A-Z0-9]{3}$";

            if (!Regex.IsMatch(value.ToString()!, Formato)) return false;

            return true;
        }
    }
    #endregion
}
