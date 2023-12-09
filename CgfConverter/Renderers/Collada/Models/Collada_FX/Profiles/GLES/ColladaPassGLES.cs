using System;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Rendering;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles.GLES;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "pass", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaPassGLES : ColladaPass
{

}

