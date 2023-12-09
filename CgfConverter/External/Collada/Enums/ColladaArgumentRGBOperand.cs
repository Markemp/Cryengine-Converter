using System;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum ColladaArgumentRGBOperand
{
    SRC_COLOR,
    ONE_MINUS_SRC_COLOR,
    SRC_ALPHA,
    ONE_MINUS_SRC_ALPHA
}

