using System;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRootAttribute(ElementName = "modifier", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaModifier
{
    [XmlText()]
    [System.ComponentModel.DefaultValueAttribute(ColladaModifierValue.CONST)]
    public ColladaModifierValue Value;
}

