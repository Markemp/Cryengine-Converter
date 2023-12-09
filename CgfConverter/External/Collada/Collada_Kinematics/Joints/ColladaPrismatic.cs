using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "prismatic", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaPrismatic
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlElement(ElementName = "axis")]
    public ColladaSIDFloatArrayString Axis;

    [XmlElement(ElementName = "limits")]
    public ColladaKinematicsLimits Limits;


}

