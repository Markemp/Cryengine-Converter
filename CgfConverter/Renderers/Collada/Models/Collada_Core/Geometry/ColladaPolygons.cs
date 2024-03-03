using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Custom_Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Geometry;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaPolygons : ColladaGeometryCommonFields
{

    [XmlElement(ElementName = "ph")]
    public ColladaPolyPH[] PH;

}

