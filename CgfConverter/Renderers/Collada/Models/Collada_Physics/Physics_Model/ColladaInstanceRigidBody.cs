using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Physics.Technique_Common;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Physics_Model;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "instance_rigid_body", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaInstanceRigidBody
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("body")]
    public string Body;

    [XmlAttribute("target")]
    public string Target;

    [XmlElement(ElementName = "technique_common")]
    public ColladaTechniqueCommonInstanceRigidBody Technique_Common;

    [XmlElement(ElementName = "technique")]
    public ColladaTechnique[] Technique;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

