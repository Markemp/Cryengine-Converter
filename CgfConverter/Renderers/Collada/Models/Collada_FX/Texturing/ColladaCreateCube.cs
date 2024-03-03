using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "create_cube", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaCreateCube
{

    [XmlElement(ElementName = "size")]
    public ColladaSizeWidthOnly Size;

    [XmlElement(ElementName = "mips")]
    public ColladaMipsAttribute Mips;

    [XmlElement(ElementName = "array")]
    public ColladaArrayLength Array_Length;

    [XmlElement(ElementName = "format")]
    public ColladaFormat Format;

    [XmlElement(ElementName = "init_from")]
    public ColladaInitFrom[] Init_From;
}

