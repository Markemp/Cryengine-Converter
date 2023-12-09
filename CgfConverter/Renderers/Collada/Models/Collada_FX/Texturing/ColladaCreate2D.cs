using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "create_2d", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaCreate2D
{
    [XmlElement(ElementName = "size_exact")]
    public ColladaSize2D Size_Exact;

    [XmlElement(ElementName = "size_ratio")]
    public ColladaSizeRatio Size_Ratio;

    [XmlElement(ElementName = "mips")]
    public ColladaMipsAttribute Mips;

    [XmlElement(ElementName = "unnormalized")]
    public XmlElement Unnormalized;

    [XmlElement(ElementName = "array")]
    public ColladaArrayLength Array_Length;


    [XmlElement(ElementName = "format")]
    public ColladaFormat Format;

    [XmlElement(ElementName = "init_from")]
    public ColladaInitFrom[] Init_From;
}

