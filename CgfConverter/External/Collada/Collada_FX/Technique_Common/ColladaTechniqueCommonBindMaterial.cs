using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaTechniqueCommonBindMaterial : Grendgine_Collada_Technique_Common
    {

        [XmlElement(ElementName = "instance_material")]
        public ColladaInstanceMaterialGeometry[] Instance_Material;

    }
}

