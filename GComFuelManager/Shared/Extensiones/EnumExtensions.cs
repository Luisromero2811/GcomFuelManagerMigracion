using System.ComponentModel;
using System.Reflection;

namespace GComFuelManager.Shared.Extensiones;
public static class EnumExtensions
{
    public static string Description(this Enum value)
    {
        Type? type = value.GetType();
        string name = Enum.GetName(type, value) ?? string.Empty;

        if (!string.IsNullOrEmpty(name))
        {
            FieldInfo? field = type.GetField(name);
            if (field is not null)
            {
                DescriptionAttribute? description = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (description is not null)
                {
                    return description.Description;
                }
            }
        }

        return string.Empty;
    }
}

