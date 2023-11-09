using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "instance_material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Instance_Material_Rendering
    {
        [XmlAttribute("url")]
        public string URL;

        [XmlElement(ElementName = "technique_override")]
        public Grendgine_Collada_Technique_Override Technique_Override;

        [XmlElement(ElementName = "bind")]
        public Grendgine_Collada_Bind_FX[] Bind;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

