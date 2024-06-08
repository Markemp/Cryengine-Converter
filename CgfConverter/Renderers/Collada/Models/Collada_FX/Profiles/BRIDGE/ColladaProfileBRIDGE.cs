using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles.BRIDGE;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "profile_BRIDGE", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaProfileBRIDGE : ColladaProfile
{
    [XmlAttribute("platform")]
    public string Platform;

    [XmlAttribute("url")]
    public string URL;
}

