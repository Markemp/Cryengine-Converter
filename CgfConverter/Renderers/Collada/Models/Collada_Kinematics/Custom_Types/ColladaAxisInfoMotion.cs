using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Articulated_Systems;
using CgfConverter.Renderers.Collada.Collada.Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Custom_Types;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaAxisInfoMotion : ColladaAxisInfo
{

    [XmlElement(ElementName = "bind")]
    public ColladaBind[] Bind;

    [XmlElement(ElementName = "newparam")]
    public ColladaNewParam[] New_Param;

    [XmlElement(ElementName = "setparam")]
    public ColladaNewParam[] Set_Param;

    [XmlElement(ElementName = "speed")]
    public ColladaCommonFloatOrParamType Speed;

    [XmlElement(ElementName = "acceleration")]
    public ColladaCommonFloatOrParamType Acceleration;

    [XmlElement(ElementName = "deceleration")]
    public ColladaCommonFloatOrParamType Deceleration;

    [XmlElement(ElementName = "jerk")]
    public ColladaCommonFloatOrParamType Jerk;



}

