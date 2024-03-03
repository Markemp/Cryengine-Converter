using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Rendering;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "stencil_clear", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaStencilClear
{
    [XmlAttribute("index")]
    [System.ComponentModel.DefaultValue(typeof(int), "0")]
    public int Index;
}
