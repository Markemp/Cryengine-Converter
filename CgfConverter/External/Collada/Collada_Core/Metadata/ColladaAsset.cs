using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaAsset
{
    [XmlElement(ElementName = "created")]
    public System.DateTime Created;

    [XmlElement(ElementName = "modified")]
    public System.DateTime Modified;

    [XmlElement(ElementName = "unit")]
    public ColladaAssetUnit Unit;

    [XmlElement(ElementName = "up_axis")]
    [System.ComponentModel.DefaultValueAttribute("Y_UP")]
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

