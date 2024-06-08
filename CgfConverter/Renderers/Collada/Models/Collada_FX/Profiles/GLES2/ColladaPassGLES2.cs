using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Rendering;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles.GLES2;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "pass", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaPassGLES2 : ColladaPass
{

    [XmlElement(ElementName = "program")]
    public ColladaProgramGLES2 Program;
}

