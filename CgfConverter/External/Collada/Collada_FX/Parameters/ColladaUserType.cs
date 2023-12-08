using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRootAttribute(ElementName = "usertype", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaUserType
{
    [XmlAttribute("typename")]
    public string TypeName;

    [XmlAttribute("source")]
    public string Source;

    [XmlElement(ElementName = "setparam")]
    public ColladaSetParam[] SetParam;
}

