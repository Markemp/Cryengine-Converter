using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Physics.Technique_Common;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Physics_Model;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "rigid_body", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaRigidBody
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("name")]
    public string Name;


    [XmlElement(ElementName = "technique_common")]
    public ColladaTechniqueCommonRigidBody Technique_Common;

    [XmlElement(ElementName = "technique")]
    public ColladaTechnique[] Technique;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

