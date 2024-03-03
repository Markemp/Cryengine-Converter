using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "image", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
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
        public ColladaCreate2D Create_2D;

        [XmlElement(ElementName = "create_3d")]
        public ColladaCreate3D Create_3D;

        [XmlElement(ElementName = "create_cube")]
        public ColladaCreateCube Create_Cube;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

