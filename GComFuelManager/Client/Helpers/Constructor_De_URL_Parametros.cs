using System;
namespace GComFuelManager.Client.Helpers
{
	public class Constructor_De_URL_Parametros
	{
		public Constructor_De_URL_Parametros()
		{
		}
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
            catch (FormatException e)
            {
                return "";
            }
            catch (ArgumentNullException e)
            {
                return "";
            }
            catch (Exception e)
            {
                return "";
            }
        }
    }
}

