using System;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "sampler3D", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaSampler3D : ColladaFXSamplerCommon
{

}

