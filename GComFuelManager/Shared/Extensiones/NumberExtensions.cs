namespace GComFuelManager.Shared.Extensiones
{
    public static class NumberExtensions
    {
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
