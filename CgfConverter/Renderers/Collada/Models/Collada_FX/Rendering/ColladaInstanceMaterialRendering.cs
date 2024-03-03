using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Materials;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Rendering;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "instance_material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaInstanceMaterialRendering
{
    [XmlAttribute("url")]
    public string URL;

    [XmlElement(ElementName = "technique_override")]
    public ColladaTechniqueOverride Technique_Override;

    [XmlElement(ElementName = "bind")]
    public ColladaBindFX[] Bind;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

