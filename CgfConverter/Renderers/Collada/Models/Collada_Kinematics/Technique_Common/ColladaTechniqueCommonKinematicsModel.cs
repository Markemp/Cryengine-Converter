using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Mathematics;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Joints;
using CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Kinematics_Models;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Technique_Common;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaTechniqueCommonKinematicsModel : ColladaTechniqueCommon
{
    [XmlElement(ElementName = "newparam")]
    public ColladaNewParam[] New_Param;

    [XmlElement(ElementName = "joint")]
    public ColladaJoint[] Joint;

    [XmlElement(ElementName = "instance_joint")]
    public ColladaInstanceJoint[] Instance_Joint;

    [XmlElement(ElementName = "link")]
    public ColladaLink[] Link;

    [XmlElement(ElementName = "formula")]
    public ColladaFormula[] Formula;

    [XmlElement(ElementName = "instance_formula")]
    public ColladaInstanceFormula[] Instance_Formula;

}

