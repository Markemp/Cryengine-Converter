using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;
using CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Custom_Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Joints;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "prismatic", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaPrismatic
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlElement(ElementName = "axis")]
    public ColladaSIDFloatArrayString Axis;

    [XmlElement(ElementName = "limits")]
    public ColladaKinematicsLimits Limits;


}

