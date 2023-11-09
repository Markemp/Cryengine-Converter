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
        public ColladaParam[] Param;

        [XmlElement(ElementName = "technique_common")]
        public ColladaTechniqueCommonBindMaterial Technique_Common;

        [XmlElement(ElementName = "technique")]
        public ColladaTechnique[] Technique;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

