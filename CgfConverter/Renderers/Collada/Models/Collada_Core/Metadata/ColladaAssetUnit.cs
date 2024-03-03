using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaAssetUnit
{
    [XmlAttribute("meter")]
    //[System.ComponentModel.DefaultValueAttribute(1.0)]        // Commented out to force it to write these values.
    public double Meter;

    [XmlAttribute("name")]
    //[System.ComponentModel.DefaultValueAttribute("meter")]
    public string Name;


}

