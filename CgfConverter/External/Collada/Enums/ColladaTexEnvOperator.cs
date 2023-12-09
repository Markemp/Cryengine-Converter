using System;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum ColladaTexEnvOperator
{
    REPLACE,
    MODULATE,
    DECAL,
    BLEND,
    ADD
}

