using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaPolygons : ColladaGeometryCommonFields
{

    [XmlElement(ElementName = "ph")]
    public ColladaPolyPH[] PH;

}

