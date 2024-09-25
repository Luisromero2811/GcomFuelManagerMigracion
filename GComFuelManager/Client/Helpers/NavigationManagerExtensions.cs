using Microsoft.AspNetCore.Components;

namespace GComFuelManager.Client.Helpers
{
    public static class NavigationManagerExtensions
    {
        public static Dictionary<string, string> ObtenerQueryString(this NavigationManager navigation, string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.Contains('?') || url[^1..] == "?")
                return new();

            var queryString = url.Split(new string[] { "?" }, StringSplitOptions.None)[1];
            var dicQueryString = queryString.Split("&")
                .ToDictionary(c => c.Split("=")[0], c => Uri.UnescapeDataString(c.Split("=")[1]));
            return dicQueryString;
        }
    }
}
