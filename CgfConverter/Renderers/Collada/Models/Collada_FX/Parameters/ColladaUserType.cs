using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Parameters;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "usertype", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaUserType
{
    [XmlAttribute("typename")]
    public string TypeName;

    [XmlAttribute("source")]
    public string Source;

    [XmlElement(ElementName = "setparam")]
    public ColladaSetParam[] SetParam;
}

