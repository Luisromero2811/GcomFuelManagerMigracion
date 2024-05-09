using GComFuelManager.Server.Helpers;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Filtro;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Net;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador Sistema, Activos Fijos")]
    [ApiController]
    public class ActivoController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ActivoController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Obtener_Activos([FromQuery] Activo_Fijo activo)
        {
            try
            {
                if (activo is null) { return BadRequest(); }

                var activos = context.Activo_Fijo
                    .Include(x => x.Conjunto)
                    .Include(x => x.Condicion)
                    .Include(x => x.Tipo)
                    .Include(x => x.Unidad)
                    .Include(x => x.Origen)
                    .Include(x => x.Etiqueta)
                    .Where(x => x.Activo)
                    .OrderByDescending(x => x.Id)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(activo.Nombre) || !string.IsNullOrWhiteSpace(activo.Nombre))
                    activos = activos.Where(x => x.Nombre.ToLower().Contains(activo.Nombre.ToLower()));

                if (!string.IsNullOrEmpty(activo.Nro_Activo) || !string.IsNullOrWhiteSpace(activo.Nro_Activo))
                    activos = activos.Where(x => x.Nro_Activo.ToLower().Contains(activo.Nro_Activo.ToLower()));

                if (!string.IsNullOrEmpty(activo.Nro_Etiqueta) || !string.IsNullOrWhiteSpace(activo.Nro_Etiqueta))
                    activos = activos.Where(x => x.Nro_Etiqueta.ToLower().Contains(activo.Nro_Etiqueta.ToLower()));

                if (activo.Conjunto_Activo != 0)
                    activos = activos.Where(x => x.Conjunto_Activo == activo.Conjunto_Activo);

                if (activo.Condicion_Activo != 0)
                    activos = activos.Where(x => x.Condicion_Activo == activo.Condicion_Activo);

                if (activo.Tipo_Activo != 0)
                    activos = activos.Where(x => x.Tipo_Activo == activo.Tipo_Activo);

                if (activo.Unidad_Medida != 0)
                    activos = activos.Where(x => x.Unidad_Medida == activo.Unidad_Medida);

                if (activo.Origen_Activo != 0)
                    activos = activos.Where(x => x.Origen_Activo == activo.Origen_Activo);

                if (activo.Etiquetado_Activo != 0)
                    activos = activos.Where(x => x.Etiquetado_Activo == activo.Etiquetado_Activo);

                if (activo.Dto_Responsable != 0)
                    activos = activos.Where(x => x.Dto_Responsable == activo.Dto_Responsable);

                await HttpContext.InsertarParametrosPaginacion(activos, activo.Registros_por_pagina, activo.Pagina);

                if (HttpContext.Response.Headers.TryGetValue("pagina", out Microsoft.Extensions.Primitives.StringValues value))
                {
                    if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value) && value != activo.Pagina)
                    {
                        activo.Pagina = int.Parse(value!);
                    }
                }

                activos = activos.Skip((activo.Pagina - 1) * activo.Registros_por_pagina).Take(activo.Registros_por_pagina);

                return Ok(activos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Guardar_Activo([FromBody] Activo_Fijo activo_Fijo)
        {
            try
            {
                if (activo_Fijo is null) { return BadRequest(); }

                activo_Fijo.Conjunto = null!;
                activo_Fijo.Condicion = null!;
                activo_Fijo.Tipo = null!;
                activo_Fijo.Unidad = null!;
                activo_Fijo.Origen = null!;
                activo_Fijo.Etiqueta = null!;
                activo_Fijo.Departamento_Responsable = null!;

                if (activo_Fijo.Id == 0)
                {

                    //var consecutivo = context.Consecutivo.FirstOrDefault(x => x.Nombre.Equals("Activo_Fijo"));
                    //if (consecutivo is null)
                    //{
                    //    Consecutivo Nuevo_Consecutivo = new() { Numeracion = 1, Nombre = "Activo_Fijo" };
                    //    context.Add(Nuevo_Consecutivo);
                    //    await context.SaveChangesAsync();
                    //    consecutivo = Nuevo_Consecutivo;
                    //}
                    //else
                    //{
                    //    consecutivo.Numeracion++;
                    //    context.Update(consecutivo);
                    //    await context.SaveChangesAsync();
                    //}

                    var consecutivo = context.Activo_Fijo.OrderBy(x => x.Numeracion).LastOrDefault(x => x.Activo);
                    if (consecutivo is null) { consecutivo = new() { Numeracion = 1 }; }
                    else { consecutivo.Numeracion++; }

                    var conjunto = context.Catalogo_Fijo.FirstOrDefault(x => x.Id == activo_Fijo.Conjunto_Activo);
                    if (conjunto is null) { return BadRequest(); }

                    activo_Fijo.Nro_Activo = $"{conjunto.Valor.Trim()}{consecutivo.Numeracion:00000}";
                    activo_Fijo.Numeracion = consecutivo.Numeracion;

                    //if (string.IsNullOrEmpty(activo_Fijo.Nro_Etiqueta) || string.IsNullOrWhiteSpace(activo_Fijo.Nro_Etiqueta))
                    //{
                    //    var origen = context.Catalogo_Fijo.FirstOrDefault(x => x.Id == activo_Fijo.Origen_Activo);
                    //    if (origen is null) { return BadRequest(); }

                    //    activo_Fijo.Nro_Etiqueta = $"{origen.Abreviacion}-OF. OPER-{activo_Fijo.Nro_Activo}";
                    //}

                    context.Add(activo_Fijo);
                }
                else
                {
                    var conjunto = context.Catalogo_Fijo.FirstOrDefault(x => x.Id == activo_Fijo.Conjunto_Activo);
                    if (conjunto is null) { return BadRequest(); }

                    activo_Fijo.Nro_Activo = $"{conjunto.Valor.Trim()}{activo_Fijo.Numeracion:00000}";

                    //if (!string.IsNullOrEmpty(activo_Fijo.Nro_Etiqueta) || string.IsNullOrWhiteSpace(activo_Fijo.Nro_Etiqueta))
                    //{
                    //    var origen = context.Catalogo_Fijo.FirstOrDefault(x => x.Id == activo_Fijo.Origen_Activo);
                    //    if (origen is null) { return BadRequest(); }

                    //    activo_Fijo.Nro_Etiqueta = $"{conjunto.Abreviacion}-OF. OPER-{activo_Fijo.Nro_Activo}";
                    //}

                    context.Update(activo_Fijo);
                }
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/conjunto")]
        public ActionResult Obtener_Catalogo_Conjunto()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Conjunto"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para conjuntos"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/condicion")]
        public ActionResult Obtener_Catalogo_Condicion()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Condicion"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para condiciones de activos"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/tipo")]
        public ActionResult Obtener_Catalogo_Tipo()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Tipo"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para tipo"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/unidad")]
        public ActionResult Obtener_Catalogo_Unidad()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Unidad_Medida"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para unidades de medida"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/origen")]
        public ActionResult Obtener_Catalogo_Origen()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Origen"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para origen"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/etiqueta")]
        public ActionResult Obtener_Catalogo_Etiqueta()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Etiqueta"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para etiqueta"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{Id:int}")]
        public async Task<ActionResult> Eliminar_Activo([FromRoute] int Id)
        {
            try
            {
                var activo = context.Activo_Fijo.Find(Id);
                if (activo is null) { return NotFound(); }

                activo.Activo = false;

                context.Update(activo);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("formato")]
        public ActionResult Descargar_Formato()
        {
            try
            {
                List<Activos_Fijos_Excel> activos = new();
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                ExcelPackage package = new();

                package.Workbook.Worksheets.Add("Registros");

                ExcelWorksheet ws = package.Workbook.Worksheets.First();
                ws.Cells["A1"].LoadFromCollection(activos, c =>
                {
                    c.PrintHeaders = true;
                    c.TableStyle = TableStyles.Medium15;
                });

                ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();

                ws = Set_Selectores(ws);

                return Ok(package.GetAsByteArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("exportar")]
        public ActionResult Exportar_Excel([FromQuery] Activo_Fijo activo)
        {
            try
            {
                if (activo is null) { return BadRequest(); }

                var activos = context.Activo_Fijo
                    .Include(x => x.Conjunto)
                    .Include(x => x.Condicion)
                    .Include(x => x.Tipo)
                    .Include(x => x.Unidad)
                    .Include(x => x.Origen)
                    .Include(x => x.Etiqueta)
                    .Where(x => x.Activo)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(activo.Nombre) || !string.IsNullOrWhiteSpace(activo.Nombre))
                    activos = activos.Where(x => x.Nombre.ToLower().Contains(activo.Nombre.ToLower()));

                if (!string.IsNullOrEmpty(activo.Nro_Activo) || !string.IsNullOrWhiteSpace(activo.Nro_Activo))
                    activos = activos.Where(x => x.Nro_Activo.ToLower().Contains(activo.Nro_Activo.ToLower()));

                if (!string.IsNullOrEmpty(activo.Nro_Etiqueta) || !string.IsNullOrWhiteSpace(activo.Nro_Etiqueta))
                    activos = activos.Where(x => x.Nro_Etiqueta.ToLower().Contains(activo.Nro_Etiqueta.ToLower()));

                if (activo.Conjunto_Activo != 0)
                    activos = activos.Where(x => x.Conjunto_Activo == activo.Conjunto_Activo);

                if (activo.Condicion_Activo != 0)
                    activos = activos.Where(x => x.Condicion_Activo == activo.Condicion_Activo);

                if (activo.Tipo_Activo != 0)
                    activos = activos.Where(x => x.Tipo_Activo == activo.Tipo_Activo);

                if (activo.Unidad_Medida != 0)
                    activos = activos.Where(x => x.Unidad_Medida == activo.Unidad_Medida);

                if (activo.Origen_Activo != 0)
                    activos = activos.Where(x => x.Origen_Activo == activo.Origen_Activo);

                if (activo.Etiquetado_Activo != 0)
                    activos = activos.Where(x => x.Etiquetado_Activo == activo.Etiquetado_Activo);

                if (activo.Dto_Responsable != 0)
                    activos = activos.Where(x => x.Dto_Responsable == activo.Dto_Responsable);

                var activos_dto = activos.Select(x => new Activos_Fijos_Excel()
                {
                    Nombre = x.Nombre,
                    Origen = x.Obtener_Origen,
                    Nro_Activo = x.Nro_Activo,
                    Conjunto = x.Obtener_Conjunto,
                    Condicion = x.Obtener_Condicion,
                    Tipo = x.Obtener_Tipo,
                    Unidad = x.Obtener_Unidad,
                    Nro_Etiqueta = x.Nro_Etiqueta,
                    Etiquetado = x.Obtener_Etiqueta,
                    Dto_Responsable = x.Obtener_Departamento
                });

                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage package = new();

                var ws = package.Workbook.Worksheets.Add("Registros");

                ws.Cells["A1"].LoadFromCollection(activos_dto, c =>
                {
                    c.PrintHeaders = true;
                    c.TableStyle = TableStyles.Medium15;
                });

                ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();

                ws = Set_Selectores(ws);

                return Ok(package.GetAsByteArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("file")]
        public async Task<ActionResult> Subir_Activos([FromForm] IEnumerable<IFormFile> files)
        {
            try
            {
                List<Activo_Fijo> activos = new();
                var MaxAllowedFiles = 10;
                var MaxAllowedSize = 1024 * 1024 * 15;
                var FilesProcesed = 0;

                List<UploadResult> uploadResults = new();

                foreach (var file in files)
                {
                    var uploadResult = new UploadResult();

                    var unthrustFileName = file.FileName;
                    var thrustFileName = WebUtility.HtmlDecode(unthrustFileName);
                    uploadResult.FileName = thrustFileName;

                    if (FilesProcesed < MaxAllowedFiles)
                    {
                        if (file.Length == 0)
                        {
                            uploadResult.ErrorCode = 2;
                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {uploadResult.FileName} : Archivo vacio.";
                            return BadRequest(uploadResult.ErrorMessage);
                        }
                        else if (file.Length > MaxAllowedSize)
                        {
                            uploadResult.ErrorCode = 3;
                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {uploadResult.FileName} : {file.Length / 1000000} Mb es mayor a la capacidad permitida ({Math.Round((double)(MaxAllowedSize / 1000000))}) Mb ";
                            return BadRequest(uploadResult.ErrorMessage);
                        }
                        else
                        {
                            using var stream = new MemoryStream();
                            await file.CopyToAsync(stream);

                            ExcelPackage.LicenseContext = LicenseContext.Commercial;
                            ExcelPackage package = new();

                            package.Load(stream);

                            if (package.Workbook.Worksheets.Count > 0)
                            {
                                using ExcelWorksheet ws = package.Workbook.Worksheets.First();

                                for (int r = 2; r < (ws.Dimension.End.Row + 1); r++)
                                {
                                    var rows = ws.Cells[r, 1, r, 10].ToList();
                                    if (rows.Count > 0)
                                    {

                                        if (ws.Cells[r, 1].Value is null) { return BadRequest($"El nombre del activo no puede estar vacio. (fila: {r}, columna: 1)"); }
                                        var nombre_activo = ws.Cells[r, 1].Value.ToString();
                                        if (string.IsNullOrEmpty(nombre_activo) || string.IsNullOrWhiteSpace(nombre_activo)) { return BadRequest($"El nombre del activo no puede estar vacio. (fila: {r}, columna: 1)"); }

                                        if (ws.Cells[r, 2].Value is null) { return BadRequest($"El origen del activo no puede estar vacio. (fila: {r}, columna: 2)"); }
                                        var origen_activo = ws.Cells[r, 2].Value.ToString();
                                        if (string.IsNullOrEmpty(origen_activo) || string.IsNullOrWhiteSpace(origen_activo)) { return BadRequest($"El origen del activo no puede estar vacio. (fila: {r}, columna: 2)"); }
                                        var id_origen = context.Catalogo_Fijo.FirstOrDefault(x => x.Valor.Equals(origen_activo));
                                        if (id_origen is null) { return BadRequest($"No se encontro el origen. (fila: {r}, columna: 2)"); }

                                        if (ws.Cells[r, 4].Value is null) { return BadRequest($"El conjunto del activo no puede estar vacio. (fila: {r}, columna: 4)"); }
                                        var conjunto_activo = ws.Cells[r, 4].Value.ToString();
                                        if (string.IsNullOrEmpty(conjunto_activo) || string.IsNullOrWhiteSpace(conjunto_activo)) { return BadRequest($"El conjunto del activo no puede estar vacio. (fila: {r}, columna: 4)"); }
                                        var id_conjunto = context.Catalogo_Fijo.FirstOrDefault(x => x.Valor.Equals(conjunto_activo));
                                        if (id_conjunto is null) { return BadRequest($"No se encontro el conjunto. (fila: {r}, columna: 4)"); }

                                        if (ws.Cells[r, 5].Value is null) { return BadRequest($"La condicion del activo no puede estar vacia. (fila: {r}, columna: 5)"); }
                                        var condicion_activo = ws.Cells[r, 5].Value.ToString();
                                        if (string.IsNullOrEmpty(condicion_activo) || string.IsNullOrWhiteSpace(condicion_activo)) { return BadRequest($"La condicion del activo no puede estar vacia. (fila: {r}, columna: 5)"); }
                                        var id_condicion = context.Catalogo_Fijo.FirstOrDefault(x => x.Valor.Equals(condicion_activo));
                                        if (id_condicion is null) { return BadRequest($"No se encontro la condicion. (fila: {r}, columna: 5)"); }

                                        if (ws.Cells[r, 6].Value is null) { return BadRequest($"El tipo del activo no puede estar vacio. (fila: {r}, columna: 6)"); }
                                        var tipo_activo = ws.Cells[r, 6].Value.ToString();
                                        if (string.IsNullOrEmpty(tipo_activo) || string.IsNullOrWhiteSpace(tipo_activo)) { return BadRequest($"El tipo del activo no puede estar vacio. (fila: {r}, columna: 6)"); }
                                        var id_tipo = context.Catalogo_Fijo.FirstOrDefault(x => x.Valor.Equals(tipo_activo));
                                        if (id_tipo is null) { return BadRequest($"No se encontro el tipo. (fila: {r}, columna: 6)"); }

                                        if (ws.Cells[r, 7].Value is null) { return BadRequest($"La unidad de medida del activo no puede estar vacia. (fila: {r}, columna: 7)"); }
                                        var unidad_activo = ws.Cells[r, 7].Value.ToString();
                                        if (string.IsNullOrEmpty(unidad_activo) || string.IsNullOrWhiteSpace(unidad_activo)) { return BadRequest($"La unidad de medida del activo no puede estar vacia. (fila: {r}, columna: 7)"); }
                                        var id_unidad = context.Catalogo_Fijo.FirstOrDefault(x => x.Valor.Equals(unidad_activo));
                                        if (id_unidad is null) { return BadRequest($"No se encontro la unidad de medida. (fila: {r}, columna: 7)"); }

                                        var etiquetado = string.Empty;
                                        if (ws.Cells[r, 9].Value is null) { etiquetado = "SI"; }
                                        else { etiquetado = ws.Cells[r, 9].Value.ToString(); }
                                        if (string.IsNullOrEmpty(etiquetado) || string.IsNullOrWhiteSpace(etiquetado)) { etiquetado = "SI"; }
                                        var id_etiquetado = context.Catalogo_Fijo.FirstOrDefault(x => x.Valor.Equals(etiquetado));
                                        if (id_etiquetado is null) { return BadRequest($"No se encontro el etiquetado. (fila: {r}, columna: 9)"); }

                                        var id_departamento = 0;
                                        if (ws.Cells[r, 10].Value is not null)
                                        {
                                            var dto_responsable = ws.Cells[r, 10].Value.ToString();
                                            if (!string.IsNullOrEmpty(etiquetado) || !string.IsNullOrWhiteSpace(dto_responsable))
                                            {
                                                var departamento = context.Catalogo_Fijo.FirstOrDefault(x => x.Valor.Equals(dto_responsable));
                                                if (departamento is null) { return BadRequest($"No se encontro el departamento responsable. (fila: {r}, columna: 10)"); }
                                                id_departamento = departamento.Id;
                                            }
                                        }

                                        var numero_activo = string.Empty;
                                        if (ws.Cells[r, 3].Value is null) { return BadRequest($"El numero de arctivo no puede estar vacio. (fila: {r}, columna: 3)"); }
                                        numero_activo = ws.Cells[r, 3].Value.ToString();
                                        if (string.IsNullOrEmpty(numero_activo) || string.IsNullOrWhiteSpace(numero_activo))
                                        { return BadRequest($"El numero de arctivo no puede estar vacio. (fila: {r}, columna: 3)"); }

                                        var string_numero = numero_activo.Replace(conjunto_activo, "");
                                        if (int.TryParse(string_numero, out int nro_consecutivo))
                                        {
                                            var string_prefijo = numero_activo.Replace(string_numero, "");
                                            numero_activo = $"{string_prefijo}{nro_consecutivo:00000}";
                                        }

                                        if (ws.Cells[r, 8].Value is null) { return BadRequest($"El numero de etiqueta no puede estar vacio. (fila: {r}, columna: 8)"); }
                                        if (string.IsNullOrEmpty(ws.Cells[r, 8].Value.ToString()) || string.IsNullOrWhiteSpace(ws.Cells[r, 8].Value.ToString()))
                                        { return BadRequest($"El numero de etiqueta no puede estar vacio. (fila: {r}, columna: 8)"); }

                                        var etiqueta_activo = string.Empty;
                                        etiqueta_activo = ws.Cells[r, 8].Value.ToString()!;

                                        string[] etiqueta_compuesta = etiqueta_activo.Split("-");
                                        string_numero = etiqueta_compuesta[2].Replace(conjunto_activo, "");
                                        if (int.TryParse(string_numero, out int nro_etiqueta))
                                        {
                                            var string_prefijo = etiqueta_compuesta[2].Replace(string_numero, "");
                                            etiqueta_activo = $"{etiqueta_compuesta[0]}-{etiqueta_compuesta[1]}-{string_prefijo}{nro_etiqueta:00000}";
                                        }

                                        Activo_Fijo activo_Fijo = new()
                                        {
                                            Nombre = nombre_activo,
                                            Origen_Activo = id_origen.Id,
                                            Conjunto_Activo = id_conjunto.Id,
                                            Condicion_Activo = id_condicion.Id,
                                            Tipo_Activo = id_tipo.Id,
                                            Unidad_Medida = id_unidad.Id,
                                            Etiquetado_Activo = id_etiquetado.Id,
                                            Nro_Activo = numero_activo,
                                            Nro_Etiqueta = etiqueta_activo,
                                            Dto_Responsable = id_departamento,
                                            Numeracion = nro_consecutivo,
                                        };

                                        activos.Add(activo_Fijo);
                                    }
                                }

                                context.AddRange(activos);

                                await context.SaveChangesAsync();
                            }
                        }
                    }

                    FilesProcesed++;
                }

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private ExcelWorksheet Set_Selectores(ExcelWorksheet excel)
        {
            var catalogo_origen = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Origen")) ?? throw new ArgumentNullException("Catalogo de origen");

            var catalogo_fijo_origen = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo_origen.Cod)).ToList();
            var list_origen = excel.DataValidations.AddListValidation(excel.Dimension.End.Row > 2 ? $"B2:B{excel.Dimension.End.Row}" : "B2");
            foreach (var item in catalogo_fijo_origen)
                list_origen.Formula.Values.Add(item.Valor);

            var catalogo_conjunto = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Conjunto")) ?? throw new ArgumentNullException("Catalogo de conjunto");

            var catalogo_fijo_conjunto = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo_conjunto.Cod)).ToList();
            var list_conjunto = excel.DataValidations.AddListValidation(excel.Dimension.End.Row > 2 ? $"D2:D{excel.Dimension.End.Row}" : "D2");
            foreach (var item in catalogo_fijo_conjunto)
                list_conjunto.Formula.Values.Add(item.Valor);

            var catalogo_condicion = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Condicion")) ?? throw new ArgumentNullException("Catalogo de condicion");

            var catalogo_fijo_condicion = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo_condicion.Cod)).ToList();
            var list_condicion = excel.DataValidations.AddListValidation(excel.Dimension.End.Row > 2 ? $"E2:E{excel.Dimension.End.Row}" : "E2");
            foreach (var item in catalogo_fijo_condicion)
                list_condicion.Formula.Values.Add(item.Valor);

            var catalogo_tipo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Tipo")) ?? throw new ArgumentNullException("Catalogo de tipo");

            var catalogo_fijo_tipo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo_tipo.Cod)).ToList();
            var list_tipo = excel.DataValidations.AddListValidation(excel.Dimension.End.Row > 2 ? $"F2:F{excel.Dimension.End.Row}" : "F2");
            foreach (var item in catalogo_fijo_tipo)
                list_tipo.Formula.Values.Add(item.Valor);

            var catalogo_unidad = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Unidad_Medida")) ?? throw new ArgumentNullException("Catalogo de unidad de medida");

            var catalogo_fijo_unidad = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo_unidad.Cod)).ToList();
            var list_unidad = excel.DataValidations.AddListValidation(excel.Dimension.End.Row > 2 ? $"G2:G{excel.Dimension.End.Row}" : "G2");
            foreach (var item in catalogo_fijo_unidad)
                list_unidad.Formula.Values.Add(item.Valor);

            var catalogo_etiquetado = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Etiqueta")) ?? throw new ArgumentNullException("Catalogo de etiquetado");

            var catalogo_fijo_etiquetado = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo_etiquetado.Cod)).ToList();
            var list_etiquetado = excel.DataValidations.AddListValidation(excel.Dimension.End.Row > 2 ? $"I2:I{excel.Dimension.End.Row}" : "I2");
            foreach (var item in catalogo_fijo_etiquetado)
                list_etiquetado.Formula.Values.Add(item.Valor);

            return excel;
        }
    }
}
