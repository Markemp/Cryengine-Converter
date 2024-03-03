using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;
using CgfConverter.Renderers.Collada.Collada.Collada_Physics.Technique_Common;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Physics_Material;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "physics_material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaPhysicsMaterial
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;


    [XmlElement(ElementName = "technique_common")]
    public ColladaTechniqueCommonPhysicsMaterial Technique_Common;

    [XmlElement(ElementName = "technique")]
    public ColladaTechnique[] Technique;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;

}

