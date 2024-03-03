using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Shaders;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "code", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaCode
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlText()]
    public string Value;
}

