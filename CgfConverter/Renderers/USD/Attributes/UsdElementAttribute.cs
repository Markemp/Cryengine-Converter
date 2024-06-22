using System;

namespace CgfConverter.Renderers.USD.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class UsdElementAttribute : Attribute
{
    public string ElementName { get; }

    public UsdElementAttribute(string elementName)
    {
        ElementName = elementName;
    }
}
