using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Kinematics_Models;
using CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Technique_Common;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Articulated_Systems;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "kinematics", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaKinematics
{

    [XmlElement(ElementName = "instance_kinematics_model")]
    public ColladaInstanceKinematicsModel[] Instance_Kinematics_Model;


    [XmlElement(ElementName = "technique_common")]
    public ColladaTechniqueCommonKinematics Technique_Common;

    [XmlElement(ElementName = "technique")]
    public ColladaTechnique[] Technique;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

