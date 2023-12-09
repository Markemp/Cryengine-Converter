using System;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum ColladaModifierValue
{
    CONST,
    UNIFORM,
    VARYING,
    STATIC,
    VOLATILE,
    EXTERN,
    SHARED
}

