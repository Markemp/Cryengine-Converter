using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada;


[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaArrayLength
{
    [XmlAttribute("length")]
    public int Length;

}

