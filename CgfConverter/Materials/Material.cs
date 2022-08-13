using System.Xml.Serialization;

namespace CgfConverter.Materials;

[XmlRoot(ElementName = "Material")]
public class Material
{
    [XmlElement(ElementName = "PublicParams")]
    public PublicParams? PublicParams { get; set; }

    [XmlAttribute(AttributeName = "Name")]
    public string? Name { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "MtlFlags")]
    public string? MtlFlags { get; set; }

    [XmlAttribute(AttributeName = "Shader")]
    public string? Shader { get; set; }

    [XmlAttribute(AttributeName = "GenMask")]
    public string? GenMask { get; set; }

    [XmlAttribute(AttributeName = "StringGenMask")]
    public string? StringGenMask { get; set; }

    [XmlAttribute(AttributeName = "SurfaceType")]
    public string? SurfaceType { get; set; }

    [XmlAttribute(AttributeName = "MatTemplate")]
    public string? MatTemplate { get; set; }

    [XmlAttribute(AttributeName = "Diffuse")]
    public string? Diffuse { get; set; }

    [XmlAttribute(AttributeName = "Specular")]
    public string? Specular { get; set; }

    [XmlAttribute(AttributeName = "Emissive")]
    public string? Emissive { get; set; }

    [XmlAttribute(AttributeName = "Shininess")]
    public string? Shininess { get; set; }

    [XmlAttribute(AttributeName = "Opacity")]
    public string? Opacity { get; set; }

    [XmlElement(ElementName = "Textures")]
    public Textures? Textures { get; set; }

    [XmlAttribute(AttributeName = "AlphaTest")]
    public string? AlphaTest { get; set; }
}

[XmlRoot(ElementName = "xml")]
public class Xml
{
    [XmlElement(ElementName = "Material")]
    public Material? Material { get; set; }
}


