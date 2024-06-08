using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;
using CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Transformation;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

namespace CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Curves
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class ColladaLine
    {
        [XmlElement(ElementName = "origin")]
        public ColladaOrigin Origin;

        [XmlElement(ElementName = "direction")]
        public ColladaFloatArrayString Direction;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

