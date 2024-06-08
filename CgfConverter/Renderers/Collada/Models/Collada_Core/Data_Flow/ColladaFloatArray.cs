using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaFloatArray : ColladaFloatArrayString
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("count")]
    public int Count;

    [XmlAttribute("digits")]
    [System.ComponentModel.DefaultValue(typeof(int), "6")]
    public int Digits;

    [XmlAttribute("magnitude")]
    [System.ComponentModel.DefaultValue(typeof(int), "38")]
    public int Magnitude;

}

