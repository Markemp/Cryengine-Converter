using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles.CG;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "program", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaProgramCG
{

    [XmlElement(ElementName = "shader")]
    public ColladaShaderCG[] Shader;
}

