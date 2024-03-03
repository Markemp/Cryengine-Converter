using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Mathematics;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaLibraryFormulas
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;


    [XmlElement(ElementName = "formula")]
    public ColladaFormula[] Formula;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

