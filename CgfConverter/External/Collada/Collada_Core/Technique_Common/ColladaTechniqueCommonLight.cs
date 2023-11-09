using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaTechniqueCommonLight : ColladaTechniqueCommon
{
    [XmlElement(ElementName = "ambient")]
    public ColladaAmbient Ambient;

    [XmlElement(ElementName = "directional")]
    public ColladaDirectional Directional;

    [XmlElement(ElementName = "point")]
    public ColladaPoint Point;

    [XmlElement(ElementName = "spot")]
    public ColladaSpot Spot;
}
