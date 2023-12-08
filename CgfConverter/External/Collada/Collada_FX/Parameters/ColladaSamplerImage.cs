using System;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRootAttribute(ElementName = "sampler_image", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaSamplerImage : Grendgine_Collada_Instance_Image
{

}

