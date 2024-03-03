using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Articulated_Systems;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "axis_info", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaAxisInfo
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("axis")]
    public string Axis;

}

