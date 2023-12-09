using System;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRootAttribute(ElementName = "sampler_states", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaSamplerStates : ColladaFXSamplerCommon
{

}

