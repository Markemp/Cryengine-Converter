using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles.GLES;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "profile_GLES", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaProfileGLES : ColladaProfile
{
    [XmlAttribute("platform")]
    public string Platform;

    [XmlElement(ElementName = "newparam")]
    public ColladaNewParam[] New_Param;

    [XmlElement(ElementName = "technique")]
    public ColladaTechniqueGLES[] Technique;
}

