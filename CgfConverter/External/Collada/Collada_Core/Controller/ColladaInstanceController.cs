using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class ColladaInstanceController
    {
        [XmlAttribute("sid")]
        public string sID;

        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("url")]
        public string URL;

        [XmlElement(ElementName = "bind_material")]
        public ColladaBindMaterial[] Bind_Material;

        [XmlElement(ElementName = "skeleton")]
        public Grendgine_Collada_Skeleton[] Skeleton;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

