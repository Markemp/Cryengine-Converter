using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "render", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaRender
{
    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("sid")]
    public string sid;

    [XmlAttribute("camera_node")]
    public string Camera_Node;

    [XmlElement(ElementName = "layer")]
    public string[] Layer;

    [XmlElement(ElementName = "instance_material")]
    public ColladaInstanceMaterialRendering Instance_Material;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

