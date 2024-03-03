using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaAsset
{
    [XmlElement(ElementName = "created")]
    public DateTime Created;

    [XmlElement(ElementName = "modified")]
    public DateTime Modified;

    [XmlElement(ElementName = "unit")]
    public ColladaAssetUnit Unit;

    [XmlElement(ElementName = "up_axis")]
    [System.ComponentModel.DefaultValue("Y_UP")]
    public string Up_Axis;

    [XmlElement(ElementName = "contributor")]
    public ColladaAssetContributor[] Contributor;

    [XmlElement(ElementName = "keywords")]
    public string Keywords;

    [XmlElement(ElementName = "revision")]
    public string Revision;

    [XmlElement(ElementName = "subject")]
    public string Subject;

    [XmlElement(ElementName = "title")]
    public string Title;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;

    [XmlElement(ElementName = "coverage")]
    public ColladaAssetCoverage Coverage;
}

