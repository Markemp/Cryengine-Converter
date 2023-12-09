using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "instance_joint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaInstanceJoint
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("url")]
    public string URL;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

