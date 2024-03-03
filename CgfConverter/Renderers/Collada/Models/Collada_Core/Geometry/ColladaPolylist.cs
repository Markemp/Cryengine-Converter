using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Geometry;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaPolylist : ColladaGeometryCommonFields
{
    [XmlElement(ElementName = "vcount")]
    public ColladaIntArrayString VCount;
}

