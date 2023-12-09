using System;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Enums;

[Serializable]
[XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum ColladaArgumentAlphaOperand
{
    SRC_ALPHA,
    ONE_MINUS_SRC_ALPHA
}

