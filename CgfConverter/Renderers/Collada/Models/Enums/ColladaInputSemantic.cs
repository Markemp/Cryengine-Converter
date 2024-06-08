using System;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Enums
{
    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum ColladaInputSemantic
    {
        BINORMAL,
        COLOR,
        CONTINUITY,
        IMAGE,
        INPUT,
        IN_TANGENT,
        INTERPOLATION,
        INV_BIND_MATRIX,
        JOINT,
        LINEAR_STEPS,
        MORPH_TARGET,
        MORPH_WEIGHT,
        NORMAL,
        OUTPUT,
        OUT_TANGENT,
        POSITION,
        TANGENT,
        TEXBINORMAL,
        TEXCOORD,
        TEXTANGENT,
        UV,
        VERTEX,
        WEIGHT,
    }
}

