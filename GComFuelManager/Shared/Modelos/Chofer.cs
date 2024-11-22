using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using OfficeOpenXml.Attributes;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace GComFuelManager.Shared.Modelos
{
    public class Chofer
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), EpplusIgnore]
        public int Cod { get; set; }
        [MaxLength(128), DisplayName("Nombre del Chofer")]
        public string? Den { get; set; } = string.Empty;
        [EpplusIgnore] public int? Codtransport { get; set; } = 0;
        [EpplusIgnore, NotMapped] public string? Id_Transportista { get; set; } = string.Empty;
        [MaxLength(6), EpplusIgnore]
        public string? Dricod { get; set; } = string.Empty;
        [MaxLength(128), DisplayName("Apellidos del Chofer")]
        public string? Shortden { get; set; } = string.Empty;
        [EpplusIgnore] public bool? Activo { get; set; } = true;
        [EpplusIgnore] public bool? Activo_Permanente { get; set; } = true;
        [EpplusIgnore] public short? Id_Tad { get; set; }
        [StringLength(13)]
        public string? RFC { get; set; } = string.Empty;
        [StringLength(40)]
        public string? Licencia { get; set; } = string.Empty;
        [EpplusIgnore]
        public int? Identificador { get; set; }
        [NotMapped, EpplusIgnore] public int? CodTra { get; set; }
        [NotMapped, DisplayName("Nombre completo del Chofer")]
        public string FullName
        {
            get
            {
                if (!string.IsNullOrEmpty(Den) && !string.IsNullOrEmpty(Shortden) && Den.ToLower().Equals(Shortden.ToLower()))
                    return Shortden;
                else
                    return $"{Den} {Shortden}";
            }
        }

        [NotMapped] public Transportista? Transportista { get; set; } = null!;
        [NotMapped] public Tonel? Tonel { get; set; } = null!;
        [NotMapped] public List<Tad> Terminales { get; set; } = new();
        [NotMapped, JsonIgnore] public List<Chofer_Tad> Chofer_Tads { get; set; } = new();

        public Chofer HardCopy()
        {
            return new()
            {
                Cod = Cod,
                Den = Den,
                Codtransport = Codtransport,
                Dricod = Dricod,
                Shortden = Shortden,
                Activo = Activo,
                Activo_Permanente = Activo_Permanente,
                Id_Tad = Id_Tad,
                RFC = RFC,
                Identificador = Identificador
            };
        }

        public void RFC_A_Capitalizado()
        {
            RFC = RFC?.ToUpper();
        }
        [NotMapped, EpplusIgnore]
        public string RFC_Capitales
        {
            get => RFC ?? string.Empty;
            set => RFC = value.ToUpper();
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

        public override string ToString()
        {
            return FullName;
        }
    }
}

