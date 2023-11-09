using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "bind_material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaBindMaterial
    {

        [XmlElement(ElementName = "param")]
        public Grendgine_Collada_Param[] Param;

        [XmlElement(ElementName = "technique_common")]
        public Grendgine_Collada_Technique_Common_Bind_Material Technique_Common;

        [XmlElement(ElementName = "technique")]
        public Grendgine_Collada_Technique[] Technique;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

