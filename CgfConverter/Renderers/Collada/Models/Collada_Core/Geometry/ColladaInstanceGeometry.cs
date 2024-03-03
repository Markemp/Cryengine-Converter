using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Materials;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Geometry;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaInstanceGeometry
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("url")]
    public string URL;


    [XmlElement(ElementName = "bind_material")]
    public ColladaBindMaterial[] Bind_Material;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

