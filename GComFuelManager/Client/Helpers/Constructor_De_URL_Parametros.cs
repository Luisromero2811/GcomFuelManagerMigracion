using GComFuelManager.Client.Shared;

namespace GComFuelManager.Client.Helpers
{
    public class Constructor_De_URL_Parametros
    {
        public int Pagina_actual { get; set; } = 1;
        public int Total_paginas { get; set; } = 1;
        public int Total_registros { get; set; } = 1;
        public Constructor_De_URL_Parametros() { }
        public static string Generar_URL(Dictionary<string, string> parametros)
        {
            try
            {
                if (parametros is not null)
                {
                    var DefaultValues = new List<string>() { "false", "", "0" };
                    var uri = string.Join("&", parametros.Where(x => !DefaultValues.Contains(x.Value.ToLower()))
                    .Select(x => $"{x.Key}={System.Web.HttpUtility.UrlEncode(x.Value)}").ToArray());
                    return uri;
                }
                return "";
            }
            catch (FormatException)
            {
                return "";
            }
            catch (ArgumentNullException)
            {
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static int Obt_Total_Paginas(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("paginas", out IEnumerable<string>? paginas_header))
            {
                if (paginas_header is not null)
                    if (paginas_header.Any())
                        return int.Parse(paginas_header.First().ToString());
            }

            return 0;
        }

        public static int Obt_Total_Registros(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("conteo", out IEnumerable<string>? conteo_header))
            {
                if (conteo_header is not null)
                    if (conteo_header.Any())
                        return int.Parse(conteo_header.First().ToString());
            }
            return 0;
        }

        public static int Obt_Pagina_Actual(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("pagina", out IEnumerable<string>? pagina_header))
            {
                if (pagina_header is not null)
                    if (pagina_header.Any())
                        return int.Parse(pagina_header.First().ToString());
            }
            return 0;
        }
    }
}
