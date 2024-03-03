using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Materials;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Technique_Common
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaTechniqueCommonBindMaterial : ColladaTechniqueCommon
    {

        [XmlElement(ElementName = "instance_material")]
        public ColladaInstanceMaterialGeometry[] Instance_Material;

    }
}

