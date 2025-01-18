namespace GComFuelManager.Shared.Extensiones
{
    public static class StringExtension
    {
        public static int ToInt(this string value) => int.TryParse(value, out int result) ? result : throw new ArgumentException("No se puede convertir el parametro a numero");

        public static bool IsNull(this string? value)
        {
            if (value is null) return true;
            if (value.Length == 0) return true;
            if (value.Trim().Length == 0) return true;
            return false;
        }
    }
}
