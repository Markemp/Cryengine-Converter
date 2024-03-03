using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaInputShared : ColladaInputUnshared
{
    [XmlAttribute("offset")]
    public int Offset;

    [XmlAttribute("set")]
    public int Set;

}

