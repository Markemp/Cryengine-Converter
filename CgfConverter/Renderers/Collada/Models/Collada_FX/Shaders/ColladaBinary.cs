using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Shaders;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "binary", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaBinary
{
    [XmlElement(ElementName = "ref")]
    public string Ref;

    [XmlElement(ElementName = "hex")]
    public ColladaHex Hex;
}

