using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaIntArray : ColladaIntArrayString
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("count")]
    public int Count;

    [XmlAttribute("minInclusive")]
    [System.ComponentModel.DefaultValue(typeof(int), "-2147483648")]
    public int Min_Inclusive;

    [XmlAttribute("maxInclusive")]
    [System.ComponentModel.DefaultValue(typeof(int), "2147483647")]
    public int Max_Inclusive;

}

