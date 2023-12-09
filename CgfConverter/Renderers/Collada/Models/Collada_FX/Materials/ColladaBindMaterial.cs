using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Technique_Common;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Materials
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "bind_material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
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

