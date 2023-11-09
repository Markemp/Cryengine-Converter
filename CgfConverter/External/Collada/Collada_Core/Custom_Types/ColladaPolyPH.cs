using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;


[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaPolyPH
{
    [XmlElement(ElementName = "p")]
    public ColladaIntArrayString P;

    [XmlElement(ElementName = "h")]
    public ColladaIntArrayString[] H;
}

