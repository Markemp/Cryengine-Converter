using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Articulated_Systems;
using CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Custom_Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Technique_Common;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaTechniqueCommonKinematics : ColladaTechniqueCommon
{
    [XmlElement(ElementName = "axis_info")]
    public ColladaAxisInfoKinematics[] Axis_Info;

    [XmlElement(ElementName = "frame_origin")]
    public ColladaFrameOrigin Frame_Origin;

    [XmlElement(ElementName = "frame_tip")]
    public ColladaFrameTip Frame_Tip;

    [XmlElement(ElementName = "frame_tcp")]
    public ColladaFrameTCP Frame_TCP;

    [XmlElement(ElementName = "frame_object")]
    public ColladaFrameObject Frame_Object;



}

