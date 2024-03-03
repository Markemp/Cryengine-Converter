using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;
namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Physics_Scene
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "library_physics_scenes", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaLibraryPhysicsScenes
    {
        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("name")]
        public string Name;

        [XmlElement(ElementName = "physics_scene")]
        public ColladaPhysicsScene[] Physics_Scene;

        [XmlElement(ElementName = "asset")]
        public ColladaAsset Asset;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

