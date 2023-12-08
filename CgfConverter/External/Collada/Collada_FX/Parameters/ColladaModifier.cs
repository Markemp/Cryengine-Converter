using System;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRootAttribute(ElementName = "modifier", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaModifier
{
    [XmlTextAttribute()]
    [System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_Modifier_Value.CONST)]
    public Grendgine_Collada_Modifier_Value Value;
}

