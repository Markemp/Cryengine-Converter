using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Analytical_Shape;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "plane", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaPlane
{
    [XmlElement(ElementName = "equation")]
    public ColladaFloatArrayString Equation;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

