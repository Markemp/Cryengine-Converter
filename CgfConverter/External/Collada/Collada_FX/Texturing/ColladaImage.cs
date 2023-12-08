using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "image", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaImage
    {
        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("sid")]
        public string sID;

        [XmlAttribute("name")]
        public string Name;

        [XmlElement(ElementName = "asset")]
        public ColladaAsset Asset;

        [XmlElement(ElementName = "renderable")]
        public ColladaRenderableShare Renderable_Share;

        [XmlElement(ElementName = "init_from")]
        public ColladaInitFrom Init_From;

        [XmlElement(ElementName = "create_2d")]
        public Grendgine_Collada_Create_2D Create_2D;

        [XmlElement(ElementName = "create_3d")]
        public Grendgine_Collada_Create_3D Create_3D;

        [XmlElement(ElementName = "create_cube")]
        public Grendgine_Collada_Create_Cube Create_Cube;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

