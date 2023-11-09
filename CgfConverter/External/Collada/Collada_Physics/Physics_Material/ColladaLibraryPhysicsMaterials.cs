using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "library_physics_materials", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaLibraryPhysicsMaterials
    {
        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("name")]
        public string Name;

        [XmlElement(ElementName = "physics_material")]
        public Grendgine_Collada_Physics_Material[] Physics_Material;

        [XmlElement(ElementName = "asset")]
        public ColladaAsset Asset;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;

    }
}

