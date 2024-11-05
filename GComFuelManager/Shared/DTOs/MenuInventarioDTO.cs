﻿using GComFuelManager.Shared.Enums;
using GComFuelManager.Shared.ModelDTOs;

namespace GComFuelManager.Shared.DTOs;

public class MenuInventarioDTO
{
    public string LabelMenu { get; set; } = string.Empty;
    public List<CatalogoValorDTO> Destinos { get; set; } = new();
    public List<CatalogoValorDTO> Origenes { get; set; } = new();
    public TipoInventario TipoInventario { get; set; }
    public bool EsVenta { get; set; } = false;
    public bool EsDevolucion { get; set; } = false;
    public bool MostrarMenuInventarios { get; set; } = false;
}