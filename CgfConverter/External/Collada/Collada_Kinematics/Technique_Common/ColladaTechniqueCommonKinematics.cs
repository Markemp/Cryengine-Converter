using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
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

