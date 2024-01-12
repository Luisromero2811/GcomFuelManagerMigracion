using OfficeOpenXml.Attributes;
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
        [Required(ErrorMessage = "{0} no puede estar vacio"), StringLength(13), DisplayName("RFC"), Validar_RFC]
        public string RFC_Exportador { get; set; } = string.Empty;
        [Required(ErrorMessage = "{0} no puede estar vacio", ErrorMessageResourceName = "Producto"), Validar_Cero, DisplayName("Producto")]
        public byte ID_Producto { get; set; } = 0;
        public DateTime Fecha_Actual { get; set; } = DateTime.Today;
        public DateTime Fecha_Creacion { get; set; } = DateTime.Now;
        public bool Activo { get; set; } = true;
        [Required(ErrorMessage = "{0} no puede estar vacio"), Validar_Negativos]
        public double Costo { get; set; } = 0;
        [NotMapped]
        public Producto? Producto { get; set; } = null!;
        [NotMapped]
        public List<OrdenEmbarque> Ordens { get; set; } = new();
        [NotMapped]
        public double? Litros_Totales { get; set; } = 0;
        [NotMapped]
        public double Utilidad { get; set; } = 0;

        public void RFC_A_Capitalizado()
        {
            RFC_Exportador = RFC_Exportador.ToUpper();
        }
        [NotMapped, EpplusIgnore]
        public string RFC_Capitales
        {
            get => RFC_Exportador;
            set => RFC_Exportador = value.ToUpper();
        }
    }

    #region validadores
    public class Validar_Cero : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validation)
        {
            if (value.Equals(0))
                return new ValidationResult("El valor no es valido");

            return ValidationResult.Success!;
        }
    }

    public class Validar_Negativos : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validation)
        {
            if (value != null)
            {
                if (double.TryParse(value.ToString(), out double numero) && numero < 0)
                    return new ValidationResult("No se aceptan valores negativos");
            }
            return ValidationResult.Success!;
        }
    }

    public class Validar_RFC : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validation)
        {
            if (value is null || string.IsNullOrEmpty(value.ToString()) || string.IsNullOrWhiteSpace(value.ToString())) return new ValidationResult("No se aceptan valores vacios");

            string FormatoPersonaFisica = @"^[A-Z]{4}\d{6}[A-Z0-9]{3}$";
            string FormatoPersonaMoral = @"^[A-Z]{3}\d{6}[A-Z0-9]{3}$";

            if (Regex.IsMatch(value!.ToString()!, FormatoPersonaFisica) || Regex.IsMatch(value!.ToString()!, FormatoPersonaMoral))
            { return ValidationResult.Success!; }

            return new ValidationResult("El RFC no cuenta con un formato valido.");
        }
    }
    #endregion

    #region Excel
    public class Excel_Ordenes_Pedimento
    {
        //[DisplayName("Numero de pedimento")] public string Numero_Pedimento { get; set; } = string.Empty;
        [DisplayName("Fecha de programa"), DisplayFormat(DataFormatString = "dd/MM/yyyy")]
        public string? Fecha_Programa { get; set; } = string.Empty;
        [DisplayName("Fecha de carga"), DisplayFormat(DataFormatString = "dd/MM/yyyy")]
        public string? Fecha_Carga { get; set; } = string.Empty;
        public string? Cliente { get; set; } = string.Empty;
        public string? Producto { get; set; } = string.Empty;
        public string? Destino { get; set; } = string.Empty;
        public double Precio { get; set; } = 0;
        public double Costo { get; set; } = 0;
        public double? Utilidad { get; set; } = 0;
        [DisplayName("Volumen cargado")] public double Volumen_Cargado { get; set; } = 0;
        [DisplayName("Utilidad sobre volumen")] public double? Utilidad_Sobre_Volumen { get; set; } = 0;
        public string Referencia { get; set; } = string.Empty;
        //public string RFC { get; set; } = string.Empty;
        public int? BOL { get; set; } = 0;
        public string Estado { get; set; } = string.Empty;
    }
    #endregion
}
