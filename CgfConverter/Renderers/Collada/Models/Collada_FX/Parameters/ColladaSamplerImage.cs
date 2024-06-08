using System;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Parameters;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "sampler_image", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaSamplerImage : ColladaInstanceImage
{

}

