using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Lighting;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Technique_Common;

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
