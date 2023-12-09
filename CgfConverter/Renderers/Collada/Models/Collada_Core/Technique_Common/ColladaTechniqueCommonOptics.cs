using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Camera;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Technique_Common;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaTechniqueCommonOptics : ColladaTechniqueCommon
{

    [XmlElement(ElementName = "orthographic")]
    public ColladaOrthographic Orthographic;

    [XmlElement(ElementName = "perspective")]
    public ColladaPerspective Perspective;
}

