using System;

namespace CgfConverter.Renderers.USD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class UsdPropertyAttribute : Attribute
{
    public string PropertyName { get; }

    public UsdPropertyAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}
