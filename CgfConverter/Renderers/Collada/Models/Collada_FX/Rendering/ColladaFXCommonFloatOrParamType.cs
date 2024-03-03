using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Types;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Rendering
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "fx_common_float_or_param_type", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaFXCommonFloatOrParamType
    {

        [XmlElement(ElementName = "float")]
        public ColladaSIDFloat Float;

        [XmlElement(ElementName = "param")]
        public ColladaParam Param;
    }
}

