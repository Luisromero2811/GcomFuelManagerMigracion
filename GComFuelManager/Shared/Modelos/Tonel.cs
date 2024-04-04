
using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Tonel
    {
        [Key, EpplusIgnore] public int Cod { get; set; }
        [MaxLength(30)] public string? Placa { get; set; } = string.Empty;
        [EpplusIgnore] public int? Codsyn { get; set; } = 0;
        [AllowNull, DefaultValue(""), EpplusIgnore] public string? Carid { get; set; } = string.Empty;
        [DisplayName("Nº Compartimento")] public int? Nrocom { get; set; } = 0;
        [EpplusIgnore]public int? Idcom { get; set; } = 0;
        [DisplayName("Capacidad de Compartimento")] public decimal? Capcom { get; set; } = decimal.Zero;
        [DisplayName("Nº Compartimento 2")] public int? Nrocom2 { get; set; } = 0;
        [EpplusIgnore] public int? Idcom2 { get; set; } = 0;
        [DisplayName("Capacidad de Compartimento 2")] public decimal? Capcom2 { get; set; } = decimal.Zero;
        [DisplayName("Nº Compartimento 3")] public int? Nrocom3 { get; set; } = 0;
        [EpplusIgnore] public int? Idcom3 { get; set; } = 0;
        [DisplayName("Capacidad de Compartimento 3")] public decimal? Capcom3 { get; set; } = decimal.Zero;
        [MaxLength(20)] public string? Tracto { get; set; } = string.Empty;
        [MaxLength(20)] public string? Placatracto { get; set; } = string.Empty;
        [EpplusIgnore] public bool? Activo { get; set; } = true;
        [EpplusIgnore] public bool? Gps { get; set; } = false;
        [DisplayName("Nº Compartimento 4")] public int? Nrocom4 { get; set; } = 0;
        [EpplusIgnore] public int? Idcom4 { get; set; } = 0;
        [DisplayName("Capacidad de Compartimento 4")] public int? Capcom4 { get; set; } = 0;
        [EpplusIgnore] public short? Id_Tad { get; set; }
        [MaxLength(50), DisplayName("Certificado de Calibración")] public string? Certificado_Calibracion { get; set; } = string.Empty;

        [NotMapped, JsonIgnore, EpplusIgnore] public Tad? Tad { get; set; }
        [NotMapped, EpplusIgnore] public List<Tad> Terminales { get; set; } = new();
        [NotMapped, JsonIgnore, EpplusIgnore] public List<Unidad_Tad> Unidad_Tads { get; set; } = new();
        [NotMapped, EpplusIgnore] public Transportista? Transportista { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Chofer? Chofer { get; set; } = null!;

        [EpplusIgnore] public string Den { get { return $"{Tracto} {Placatracto} {Placa} {Capcom!} {Capcom2!} {Capcom3!} {Capcom4!} {Codsyn!}"; } }
        [EpplusIgnore] public string Veh { get { return $"{Tracto} {Placa}"; } }
        [NotMapped, EpplusIgnore]
        public int CapDisponible
        {
            get
            {
                int c = 0;
                if (Capcom != 0 && Capcom != null)
                    c++;
                if (Capcom2 != 0 && Capcom2 != null)
                    c++;
                if (Capcom3 != 0 && Capcom3 != null)
                    c++;
                if (Capcom4 != 0 && Capcom4 != null)
                    c++;
                return c;
            }
        }

        [NotMapped, EpplusIgnore] public int? CodTra { get; set; }

        [NotMapped,EpplusIgnore]
        public List<CapTonel> Capacidades
        {
            get
            {
                CapTonel capTonel = new CapTonel();
                List<CapTonel> capTonels = new List<CapTonel>();

                if(Nrocom != null && Nrocom != 0)
                {
                    capTonel.CapCom = Capcom;
                    capTonel.IdCom = Idcom;
                    capTonel.NroCom = Nrocom;
                    capTonels.Add(capTonel);
                    capTonel = new CapTonel();
                }

                if (Nrocom2 != null && Nrocom2 != 0)
                {
                    capTonel.CapCom = Capcom2;
                    capTonel.IdCom = Idcom2;
                    capTonel.NroCom = Nrocom2;
                    capTonels.Add(capTonel);
                    capTonel = new CapTonel();
                }

                if (Nrocom3 != null && Nrocom3 != 0)
                {
                    capTonel.CapCom = Capcom3;
                    capTonel.IdCom = Idcom3;
                    capTonel.NroCom = Nrocom3;
                    capTonels.Add(capTonel);
                    capTonel = new CapTonel();
                }

                if (Nrocom4 != null && Nrocom4 != 0)
                {
                    capTonel.CapCom = Capcom4;
                    capTonel.IdCom = Idcom4;
                    capTonel.NroCom = Nrocom4;
                    capTonels.Add(capTonel);
                    capTonel = new CapTonel();
                }

                return capTonels;
            }
        }
    }

    public class CapTonel
    {
        [Key] public int? IdCom { get; set; } = 0;
        public decimal? CapCom { get; set; } = 0;
        public int? NroCom { get; set; } = 0;
    }
}
