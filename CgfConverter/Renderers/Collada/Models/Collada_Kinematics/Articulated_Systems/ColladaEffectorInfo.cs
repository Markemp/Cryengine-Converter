using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Articulated_Systems;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "effector_info", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaEffectorInfo
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("name")]
    public string Name;

    [XmlElement(ElementName = "bind")]
    public ColladaBind[] Bind;

    [XmlElement(ElementName = "newparam")]
    public ColladaNewParam[] New_Param;

    [XmlElement(ElementName = "setparam")]
    public ColladaSetParam[] Set_Param;

    [XmlElement(ElementName = "speed")]
    public ColladaCommonFloat2OrParamType Speed;

    [XmlElement(ElementName = "acceleration")]
    public ColladaCommonFloat2OrParamType Acceleration;

    [XmlElement(ElementName = "deceleration")]
    public ColladaCommonFloat2OrParamType Deceleration;

    [XmlElement(ElementName = "jerk")]
    public ColladaCommonFloat2OrParamType Jerk;

}

