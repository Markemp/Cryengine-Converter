using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Joints;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "joint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
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

