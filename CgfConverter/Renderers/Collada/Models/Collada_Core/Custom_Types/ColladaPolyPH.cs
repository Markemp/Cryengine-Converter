using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Custom_Types;


[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaPolyPH
{
    [XmlElement(ElementName = "p")]
    public ColladaIntArrayString P;

    [XmlElement(ElementName = "h")]
    public ColladaIntArrayString[] H;
}

