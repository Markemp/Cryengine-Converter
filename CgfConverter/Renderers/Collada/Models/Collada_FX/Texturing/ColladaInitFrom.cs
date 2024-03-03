using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;
using CgfConverter.Renderers.Collada.Collada.Enums;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "init_from", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaInitFrom
    {
        // Commented out parts are not recognized in Blender (and probably not part of Collada 1.4.1)
        //[XmlAttribute("mips_generate")]
        //public bool Mips_Generate;

        //[XmlAttribute("array_index")]
        //public int Array_Index;

        //[XmlAttribute("mip_index")]
        //public int Mip_Index;

        // Uri added to support 1.4.1 formats
        [XmlText()]
        public string Uri;

        //      [XmlAttribute("depth")]
        //public int Depth;

        [XmlAttribute("face")]
        [System.ComponentModel.DefaultValue(ColladaFace.POSITIVE_X)]
        public ColladaFace Face;

        [XmlElement(ElementName = "ref")]
        public string Ref;

        [XmlElement(ElementName = "hex")]
        public ColladaHex Hex;
    }
}

