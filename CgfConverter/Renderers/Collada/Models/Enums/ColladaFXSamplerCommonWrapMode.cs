using System;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Enums;

[Serializable]
[XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum ColladaFXSamplerCommonWrapMode
{
    WRAP,
    MIRROR,
    CLAMP,
    BORDER,
    MIRROR_ONCE,

    REPEAT,
    CLAMP_TO_EDGE,
    MIRRORED_REPEAT
}

