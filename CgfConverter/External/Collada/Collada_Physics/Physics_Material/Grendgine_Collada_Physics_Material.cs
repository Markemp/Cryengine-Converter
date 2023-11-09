using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "physics_material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Physics_Material
    {
        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("name")]
        public string Name;


        [XmlElement(ElementName = "technique_common")]
        public Grendgine_Collada_Technique_Common_Physics_Material Technique_Common;

        [XmlElement(ElementName = "technique")]
        public Grendgine_Collada_Technique[] Technique;

        [XmlElement(ElementName = "asset")]
        public ColladaAsset Asset;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;

    }
}

