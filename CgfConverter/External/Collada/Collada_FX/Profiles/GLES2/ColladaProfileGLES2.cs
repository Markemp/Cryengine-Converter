using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "profile_GLES2", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaProfileGLES2 : ColladaProfile
{
    [XmlAttribute("platform")]
    public string Platform;

    [XmlAttribute("language")]
    public string Language;

    [XmlElement(ElementName = "newparam")]
    public ColladaNewParam[] New_Param;

    [XmlElement(ElementName = "technique")]
    public ColladaTechniqueGLES2[] Technique;

    [XmlElement(ElementName = "code")]
    public ColladaCode[] Code;

    [XmlElement(ElementName = "include")]
    public ColladaInclude[] Include;
}

