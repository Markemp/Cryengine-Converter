using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "fx_common_float_or_param_type", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaFXCommonFloatOrParamType
    {

        [XmlElement(ElementName = "float")]
        public ColladaSIDFloat Float;

        [XmlElement(ElementName = "param")]
        public ColladaParam Param;
    }
}

