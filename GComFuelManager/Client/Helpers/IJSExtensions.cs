using Microsoft.JSInterop;

namespace GComFuelManager.Client.Helpers
{
    public static class IJSExtensions
    {
        public static ValueTask<object> GuardarComo(this IJSRuntime js, string nombreArchivo, byte[] archivo)
        {

            return js.InvokeAsync<object>("saveAsFile",
                nombreArchivo,
                Convert.ToBase64String(archivo));
        }

        public static bool IsZero(this double? d)
        {
            if (d is null) return false;
            if (d == 0) return true;
            return false;
        }

        public static bool IsZero(this double d)
        {
            if (d == 0) return true;
            return false;
        }

        public static bool IsZero(this int? d)
        {
            if (d is null) return false;
            if (d == 0) return true;
            return false;
        }

        public static bool IsZero(this int d)
        {
            if (d == 0) return true;
            return false;
        }
    }
}

