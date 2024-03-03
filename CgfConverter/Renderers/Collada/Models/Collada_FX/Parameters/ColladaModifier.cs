using System;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Enums;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Parameters;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "modifier", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaModifier
{
    [XmlText()]
    [System.ComponentModel.DefaultValue(ColladaModifierValue.CONST)]
    public ColladaModifierValue Value;
}

