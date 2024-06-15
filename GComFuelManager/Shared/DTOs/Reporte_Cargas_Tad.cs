using System.ComponentModel;

namespace GComFuelManager.Shared.DTOs;

public class Reporte_Cargas_Tad_SJI
{
    [DisplayName("SoldTo No.")]
    public string SoldTo { get; set; } = "ENE01";
    [DisplayName("Cust. Sold-to Name")]
    public string CustSoldToName { get; set; } = "ENERGAS DE MEXICO";
    [DisplayName("ShipTo Name")]
    public string? ShipToName { get; set; } = string.Empty;
    [DisplayName("Cust PO No.")]
    public int? CustPONo { get; set; } = 0;
    [DisplayName("SO No.")]
    public int? SONo { get; set; } = 0;
    [DisplayName("SO Create Dt")]
    public DateTime? SOCreateDt { get; set; } = DateTime.Today;
    [DisplayName("Cust RDD")]
    public DateTime? CustRDD { get; set; } = DateTime.Today;
    [DisplayName("Vendor Plt")]
    public string? VendorPlt { get; set; } = string.Empty;
    [DisplayName("Item")]
    public string? Item { get; set; } = string.Empty;
    public string? Material { get; set; } = string.Empty;
    [DisplayName("Material Description Name")]
    public string? MaterialDescriptionName { get; set; } = string.Empty;
    [DisplayName("SO Order Qty")]
    public int? SOOrderQty { get; set; } = 0;
    [DisplayName("Sales UoM")]
    public string SaleUoM { get; set; } = "L20";
    [DisplayName("Station Name MX")]
    public string? StationNameMX { get; set; } = string.Empty;
    [DisplayName("Tank Count")]
    public string? TankCount { get; set; } = string.Empty;
    public string? EQUIPO { get; set; } = string.Empty;
    public string? OPERADOR { get; set; } = string.Empty;
    public string? TRANSPORTISTA { get; set; } = string.Empty;
    public int? BOL { get; set; } = null!;
    [DisplayName("LICENCIA OPERADOR")]
    public string LICENCIAOPERADOR { get; set; } = string.Empty;
    [DisplayName("DESCRIPCION DE CAMBIOS")]
    public string DESCRIPCIONDECAMBIOS { get; set; } = string.Empty;
}
