using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Geometry;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaGeometryCommonFields
{
    [XmlAttribute("count")]
    public int Count;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("material")]
    public string Material;

    [XmlElement(ElementName = "input")]
    public ColladaInputShared[] Input;

    [XmlElement(ElementName = "p")]
    public ColladaIntArrayString P;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;

}

