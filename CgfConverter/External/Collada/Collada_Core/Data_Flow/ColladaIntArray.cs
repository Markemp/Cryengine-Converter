using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

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
    [System.ComponentModel.DefaultValueAttribute(typeof(int), "-2147483648")]
    public int Min_Inclusive;

    [XmlAttribute("maxInclusive")]
    [System.ComponentModel.DefaultValueAttribute(typeof(int), "2147483647")]
    public int Max_Inclusive;

}

