using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Camera;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Controller;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Geometry;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Lighting;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Transform;
using CgfConverter.Renderers.Collada.Collada.Enums;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Scene;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaNode
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("type")]
    public ColladaNodeType Type;

    [XmlAttribute("layer")]
    public string Layer;

    [XmlElement(ElementName = "lookat", Order = 3)]
    public ColladaLookat[] Lookat;

    [XmlElement(ElementName = "matrix", Order = 4)]
    public ColladaMatrix[] Matrix;

    [XmlElement(ElementName = "rotate", Order = 2)]
    public ColladaRotate[] Rotate;

    [XmlElement(ElementName = "scale", Order = 5)]
    public ColladaScale[] Scale;

    [XmlElement(ElementName = "skew", Order = 6)]
    public ColladaSkew[] Skew;

    [XmlElement(ElementName = "translate", Order = 1)]
    public ColladaTranslate[] Translate;

    [XmlElement(ElementName = "instance_camera", Order = 7)]
    public ColladaInstanceCamera[] Instance_Camera;

    [XmlElement(ElementName = "instance_controller", Order = 8)]
    public ColladaInstanceController[] Instance_Controller;

    [XmlElement(ElementName = "instance_geometry", Order = 9)]
    public ColladaInstanceGeometry[] Instance_Geometry;

    [XmlElement(ElementName = "instance_light", Order = 10)]
    public ColladaInstanceLight[] Instance_Light;

    [XmlElement(ElementName = "instance_node", Order = 11)]
    public ColladaInstanceNode[] Instance_Node;

    [XmlElement(ElementName = "asset", Order = 12)]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "node", Order = 13)]
    public ColladaNode[] node;

    [XmlElement(ElementName = "extra", Order = 14)]
    public ColladaExtra[] Extra;

}

