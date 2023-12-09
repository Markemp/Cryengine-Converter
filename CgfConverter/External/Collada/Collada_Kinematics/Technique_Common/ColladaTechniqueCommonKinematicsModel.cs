using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
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

