using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "joint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaJoint
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("sid")]
    public string sID;

    [XmlElement(ElementName = "prismatic")]
    public ColladaPrismatic[] Prismatic;

    [XmlElement(ElementName = "revolute")]
    public ColladaRevolute[] Revolute;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

