using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Rendering;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "render", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
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

