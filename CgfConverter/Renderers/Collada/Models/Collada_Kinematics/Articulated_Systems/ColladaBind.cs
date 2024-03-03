using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Articulated_Systems;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "bind", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaBind
{
    [XmlAttribute("symbol")]
    public string Symbol;

    [XmlElement(ElementName = "param")]
    public ColladaParam Param;

    [XmlElement(ElementName = "float")]
    public float Float;

    [XmlElement(ElementName = "int")]
    public int Int;

    [XmlElement(ElementName = "bool")]
    public bool Bool;

    [XmlElement(ElementName = "SIDREF")]
    public string SIDREF;

}

