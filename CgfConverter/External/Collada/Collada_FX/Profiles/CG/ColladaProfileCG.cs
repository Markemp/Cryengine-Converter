using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "profile_CG", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaProfileCG : ColladaProfile
{
    [XmlAttribute("platform")]
    public string Platform;

    [XmlElement(ElementName = "newparam")]
    public ColladaNewParam[] New_Param;

    [XmlElement(ElementName = "technique")]
    public ColladaTechniqueCG[] Technique;

    [XmlElement(ElementName = "code")]
    public ColladaCode[] Code;

    [XmlElement(ElementName = "include")]
    public ColladaInclude[] Include;
}

