using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Physics_Material;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "instance_physics_material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaInstancePhysicsMaterial
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("url")]
    public string URL;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}
