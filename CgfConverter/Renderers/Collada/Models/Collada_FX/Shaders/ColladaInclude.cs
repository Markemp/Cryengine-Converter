using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Shaders;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "include", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaInclude
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("url")]
    public string URL;

}

