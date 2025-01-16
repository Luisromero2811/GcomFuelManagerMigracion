




using GComFuelManager.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Helpers
{
    public class PrecioHelper : IPrecioHelper
    {
        private readonly ApplicationDbContext context;

        public PrecioHelper(ApplicationDbContext context)
        {
            this.context = context;
        }

        public double ObtenerPrecioPorIdOrden(long? id)
        {
            try
            {
                if (id is null) { throw new ArgumentNullException(nameof(id)); }

                var orden = context.Orden.AsNoTracking().Include(x => x.OrdenEmbarque).FirstOrDefault(x => x.Cod == id);
                if (orden is null) { return 0; }

                double precio = new();

                var precioVig = context.Precio.AsNoTracking()
                    .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Id_Tad)
                    .Select(x => new { x.Pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioPro = context.PrecioProgramado.AsNoTracking()
                    .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Id_Tad)
                    .Select(x => new { x.Pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioHis = context.PreciosHistorico.AsNoTracking()
                    .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && orden.Fchcar != null && x.FchDia.Date <= orden.Fchcar.Value.Date && x.Id_Tad == orden.Id_Tad)
                    .Select(x => new { x.pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                if (precioHis is not null)
                    precio = precioHis.pre ?? 0;

                if (precioVig is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today)
                    if (precioVig.FchDia.Date == DateTime.Today.Date)
                        precio = precioVig.Pre;

                if (precioPro is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today && context.PrecioProgramado.Any())
                    if (precioPro.FchDia.Date == DateTime.Today.Date)
                        precio = precioPro.Pre;

                if (orden != null && context.OrdenPedido.Any(x => x.CodPed == orden.Cod))
                {
                    var ordenepedido = context.OrdenPedido.AsNoTracking()
                        .Where(x => x.CodPed == orden.Cod && !string.IsNullOrEmpty(x.Folio))
                        .Select(x => x.Folio)
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(ordenepedido) && !string.IsNullOrWhiteSpace(ordenepedido))
                    {
                        var cierre = context.OrdenCierre.AsNoTracking()
                            .Where(x => x.Folio == ordenepedido && x.Id_Tad == orden.Id_Tad && x.CodPrd == orden.Codprd)
                            .FirstOrDefault();

                        if (cierre is not null)
                            precio = cierre.Precio;
                    }
                }

                if (orden is not null && precioHis is null && precioPro is null && precioVig is null)
                    precio = orden.OrdenEmbarque?.Pre ?? 0;

                return precio;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<double> ObtenerPrecioPorIdOrdenAsync(long? id)
        {
            try
            {
                if (id is null) { throw new ArgumentNullException(nameof(id)); }

                var orden = await context.Orden.AsNoTracking().Include(x => x.OrdenEmbarque).FirstOrDefaultAsync(x => x.Cod == id);
                if (orden is null) { return 0; }

                double precio = new();

                var precioVig = await context.Precio.AsNoTracking()
                    .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Id_Tad)
                    .Select(x => new { x.Pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefaultAsync();

                var precioPro = await context.PrecioProgramado.AsNoTracking()
                    .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Id_Tad)
                    .Select(x => new { x.Pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefaultAsync();

                var precioHis = await context.PreciosHistorico.AsNoTracking()
                    .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && orden.Fchcar != null && x.FchDia.Date <= orden.Fchcar.Value.Date && x.Id_Tad == orden.Id_Tad)
                    .Select(x => new { x.pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefaultAsync();

                if (precioHis is not null)
                    precio = precioHis.pre ?? 0;

                if (precioVig is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today)
                    if (precioVig.FchDia.Date == DateTime.Today.Date)
                        precio = precioVig.Pre;

                if (precioPro is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today && await context.PrecioProgramado.AnyAsync())
                    if (precioPro.FchDia.Date == DateTime.Today.Date)
                        precio = precioPro.Pre;

                if (orden != null && await context.OrdenPedido.AnyAsync(x => x.CodPed == orden.Cod))
                {
                    var ordenepedido = await context.OrdenPedido.AsNoTracking()
                        .Where(x => x.CodPed == orden.Cod && !string.IsNullOrEmpty(x.Folio))
                        .Select(x => x.Folio)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(ordenepedido) && !string.IsNullOrWhiteSpace(ordenepedido))
                    {
                        var cierre = await context.OrdenCierre.AsNoTracking()
                            .Where(x => x.Folio == ordenepedido && x.Id_Tad == orden.Id_Tad && x.CodPrd == orden.Codprd)
                            .FirstOrDefaultAsync();

                        if (cierre is not null)
                            precio = cierre.Precio;
                    }
                }

                if (orden is not null && precioHis is null && precioPro is null && precioVig is null)
                    precio = orden.OrdenEmbarque?.Pre ?? 0;

                return precio;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public double ObtenerPrecioPorIdOrdenEmbarque(int? id)
        {
            try
            {
                if (id is null) { throw new ArgumentNullException(nameof(id)); }

                var orden = context.OrdenEmbarque
                    .AsNoTracking()
                    .Where(x => x.Cod == id)
                    .Include(x => x.Orden)
                    .Include(x => x.OrdenCierre)
                    .IgnoreAutoIncludes()
                    .FirstOrDefault();

                if (orden is null)
                    return 0;

                double precio = new();

                var precioVig = context.Precio.AsNoTracking()
                    .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Codtad)
                    .Select(x => new { x.Pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioPro = context.PrecioProgramado.AsNoTracking()
                    .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Codtad)
                    .Select(x => new { x.Pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioHis = context.PreciosHistorico.AsNoTracking()
                    .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && orden.Fchcar != null && x.FchDia <= DateTime.Today && x.Id_Tad == orden.Codtad)
                    .Select(x => new { x.pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                if (precioHis is not null)
                    precio = precioHis.pre ?? 0;

                if (precioVig is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today)
                    precio = precioVig.Pre;

                if (precioPro is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today && context.PrecioProgramado.Any())
                    precio = precioPro.Pre;

                if (orden != null && context.OrdenPedido.Any(x => x.CodPed == orden.Cod))
                {
                    var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == orden.Cod && !string.IsNullOrEmpty(x.Folio)).Select(x => x.Folio).FirstOrDefault();

                    if (!string.IsNullOrEmpty(ordenepedido) && !string.IsNullOrWhiteSpace(ordenepedido))
                    {
                        var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido && x.Id_Tad == orden.Codtad
                         && x.CodPrd == orden.Codprd).FirstOrDefault();

                        if (cierre is not null)
                            precio = cierre.Precio;
                    }
                }

                if (orden is not null && precioHis is null && precioPro is null && precioVig is null)
                    precio = orden.Pre ?? 0;

                return precio;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<double> ObtenerPrecioPorIdOrdenEmbarqueAsync(int? id)
        {
            try
            {
                if (id is null) { throw new ArgumentNullException(nameof(id)); }

                var orden = await context.OrdenEmbarque
                    .AsNoTracking()
                    .Where(x => x.Cod == id)
                    .Include(x => x.Orden)
                    .Include(x => x.OrdenCierre)
                    .IgnoreAutoIncludes()
                    .FirstOrDefaultAsync();

                if (orden is null)
                    return 0;

                double precio = new();

                var precioVig = await context.Precio.AsNoTracking()
                    .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Codtad)
                    .Select(x => new { x.Pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefaultAsync();

                var precioPro = await context.PrecioProgramado.AsNoTracking()
                    .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Codtad)
                    .Select(x => new { x.Pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefaultAsync();

                var precioHis = await context.PreciosHistorico.AsNoTracking()
                    .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && orden.Fchcar != null && x.FchDia <= DateTime.Today && x.Id_Tad == orden.Codtad)
                    .Select(x => new { x.pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefaultAsync();

                if (precioHis is not null)
                    precio = precioHis.pre ?? 0;

                if (precioVig is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today)
                    precio = precioVig.Pre;

                if (precioPro is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today && await context.PrecioProgramado.AnyAsync())
                    precio = precioPro.Pre;

                if (orden != null && await context.OrdenPedido.AnyAsync(x => x.CodPed == orden.Cod))
                {
                    var ordenepedido = await context.OrdenPedido.AsNoTracking()
                        .Where(x => x.CodPed == orden.Cod && !string.IsNullOrEmpty(x.Folio))
                        .Select(x => x.Folio)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(ordenepedido) && !string.IsNullOrWhiteSpace(ordenepedido))
                    {
                        var cierre = await context.OrdenCierre.AsNoTracking()
                            .Where(x => x.Folio == ordenepedido && x.Id_Tad == orden.Codtad && x.CodPrd == orden.Codprd)
                            .FirstOrDefaultAsync();

                        if (cierre is not null)
                            precio = cierre.Precio;
                    }
                }

                if (orden is not null && precioHis is null && precioPro is null && precioVig is null)
                    precio = orden.Pre ?? 0;

                return precio;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
