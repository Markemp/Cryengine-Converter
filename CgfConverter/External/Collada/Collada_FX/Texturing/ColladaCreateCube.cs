using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "create_cube", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
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

