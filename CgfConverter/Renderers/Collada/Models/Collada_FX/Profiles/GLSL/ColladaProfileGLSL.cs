using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Shaders;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles.GLSL;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "profile_GLSL", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaProfileGLSL : ColladaProfile
{
    [XmlAttribute("platform")]
    public string Platform;

    [XmlElement(ElementName = "newparam")]
    public ColladaNewParam[] New_Param;

    [XmlElement(ElementName = "technique")]
    public ColladaTechniqueGLSL[] Technique;

    [XmlElement(ElementName = "code")]
    public ColladaCode[] Code;

    [XmlElement(ElementName = "include")]
    public ColladaInclude[] Include;
}

