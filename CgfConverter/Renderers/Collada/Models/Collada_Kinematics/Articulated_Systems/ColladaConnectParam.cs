using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Articulated_Systems;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "connect_param", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaConnectParam
{
    [XmlAttribute("ref")]
    public string Ref;
}

